let calDate = new Date();
let calCourseFilter = 'ALL';

function buildCalendarHtml() {
    const year = calDate.getFullYear();
    const month = calDate.getMonth();
    const months = I18N[currentLang].months;
    const weekdays = I18N[currentLang].weekdays;

    const prevName = months[(month + 11) % 12];
    const nextName = months[(month + 1) % 12];

    const firstDay = new Date(year, month, 1).getDay(); // 0=Sun
    const startOffset = (firstDay + 6) % 7; // Mon=0
    const daysInMonth = new Date(year, month + 1, 0).getDate();
    const todayStr = new Date().toISOString().slice(0, 10);

    // Хичээлийн шүүлтүүрийн сонголтууд
    const courseOptions = studentData.enrolled.map(c =>
        `<option value="${c.name}" ${calCourseFilter === c.name ? 'selected' : ''}>${c.name}</option>`
    ).join('');

    const filteredEvents = calCourseFilter === 'ALL'
        ? calendarEvents
        : calendarEvents.filter(e => e.courseName === calCourseFilter);

    let cells = '';
    let day = 1;
    for (let row = 0; row < 6; row++) {
        let rowHtml = '';
        for (let col = 0; col < 7; col++) {
            if ((row === 0 && col < startOffset) || day > daysInMonth) {
                rowHtml += '<td></td>';
            } else {
                const dateStr = `${year}-${String(month + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
                const isToday = dateStr === todayStr;
                const evs = filteredEvents.filter(e => e.date === dateStr);

                rowHtml += `
                    <td class="${isToday ? 'cal-today' : ''}">
                        <span class="cal-day-num">${day}</span>
                        ${evs.map(e => `
                            <div class="cal-event-tag" title="${e.title}" onclick="openEventDetail(${e.id})">
                                ○ ${e.title}
                            </div>
                        `).join('')}
                    </td>`;
                day++;
            }
        }
        cells += `<tr>${rowHtml}</tr>`;
        if (day > daysInMonth) break;
    }

    return `
        <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:12px; gap:10px; flex-wrap:wrap;">
            <select class="m-select" style="width:260px;" onchange="calCourseFilter=this.value; renderCurrentLmsView();">
                <option value="ALL">${t('all_courses')}</option>
                ${courseOptions}
            </select>
            <button class="btn-primary" onclick="openEventModal()">${t('new_event')}</button>
        </div>
        <div class="cal-header">
            <button class="cal-nav" onclick="changeMonth(-1)">◂ ${prevName}</button>
            <div class="cal-month">${months[month]} ${year}</div>
            <button class="cal-nav" onclick="changeMonth(1)">${nextName} ▸</button>
        </div>
        <table class="cal-table">
            <thead>
                <tr>${weekdays.map(d => `<th>${d}</th>`).join('')}</tr>
            </thead>
            <tbody>${cells}</tbody>
        </table>
    `;
}

function changeMonth(delta) {
    calDate.setMonth(calDate.getMonth() + delta);
    renderCurrentLmsView();
}

function openEventModal() {
    document.getElementById('ev-title').value = '';
    document.getElementById('ev-date').value = new Date().toISOString().slice(0, 10);
    document.getElementById('ev-time').value = '09:00';

    const sel = document.getElementById('ev-course');
    const courses = studentData.enrolled.length > 0 ? studentData.enrolled : studentData.available;
    sel.innerHTML = `<option value="">-- Ерөнхий (General) --</option>` +
        courses.map(c => `<option value="${c.name}">${c.name}</option>`).join('');

    document.getElementById('event-modal').classList.add('open');
}

function closeEventModal() {
    document.getElementById('event-modal').classList.remove('open');
}

function saveCalendarEvent() {
    const title = document.getElementById('ev-title').value.trim();
    const courseName = document.getElementById('ev-course').value;
    const date = document.getElementById('ev-date').value;
    const time = document.getElementById('ev-time').value || '09:00';

    if (!title || !date) {
        showToast(currentLang === 'mn' ? 'Арга хэмжээний нэр болон огноог оруулна уу.' : 'Please enter title and date.');
        return;
    }

    postToCSharp({
        action: 'addCalendarEvent',
        userId: currentUser.id,
        courseName,
        title,
        date,
        time
    });

    closeEventModal();
}

// Календарийн эвент дээр дарахад гарч ирэх дэлгэрэнгүй цонх
function openEventDetail(eventId) {
    const ev = calendarEvents.find(e => e.id === eventId);
    if (!ev) return;

    document.getElementById('ed-title').innerText = ev.title;
    document.getElementById('ed-datetime').innerText = `${ev.date}, ${ev.time}`;
    document.getElementById('ed-type').innerText = ev.eventType || 'Course event';

    const courseEl = document.getElementById('ed-course');
    courseEl.innerText = ev.courseName || (currentLang === 'mn' ? 'Хувийн тэмдэглэл' : 'Personal event');
    courseEl.onclick = () => {
        if (ev.courseName) {
            closeEventDetailModal();
            openCourse(ev.courseName, ev.title);
        }
    };

    const goBtn = document.getElementById('ed-go-btn');
    goBtn.innerText = t('go_to_activity');
    goBtn.onclick = () => {
        closeEventDetailModal();
        if (ev.courseName) {
            openCourse(ev.courseName, ev.title);
        } else {
            showToast(`"${ev.title}" — ${ev.date} ${ev.time}`);
        }
    };

    document.getElementById('event-detail-modal').classList.add('open');
}

function closeEventDetailModal() {
    document.getElementById('event-detail-modal').classList.remove('open');
}