let courseViewMode = 'card';
let courseSearchText = '';
let courseSortMode = 'name';
let availSearchText = '';

function renderStudentPage(tab) {
    const wrapper = document.getElementById('lms-main-wrapper');
    let html = '';

    if (tab === 'home') {
        const filtered = studentData.available.filter(c =>
            c.name.toLowerCase().includes(availSearchText.toLowerCase()) ||
            c.department.toLowerCase().includes(availSearchText.toLowerCase())
        );

        html = `
            <div style="display:flex; justify-content:space-between; align-items:flex-end;">
                <h1 class="page-heading">${t('site_announcements')}</h1>
                <a href="#" onclick="showToast('${t('subscribe_forum')}!'); return false;" style="color:var(--green-main); font-size:14px; text-decoration:none; margin-bottom:14px;">${t('subscribe_forum')}</a>
            </div>
            <div class="info-alert">${t('no_topics')}</div>

            <div class="search-bar-wrap">
                <input type="text" placeholder="${t('search_courses')}" value="${availSearchText}" oninput="availSearchText=this.value; renderStudentPage('home');">
                <button>🔍</button>
            </div>

            <h2 class="page-heading" style="font-size:24px; margin-bottom:16px;">${t('available_courses')}</h2>
            ${filtered.length === 0 ? `<div class="moodle-card" style="color:var(--muted); text-align:center;">Хичээл олдсонгүй.</div>` :
                filtered.map(c => {
                    const already = studentData.enrolled.some(e => e.name === c.name);
                    const safeName = c.name.replace(/'/g, "\\'");
                    return `
                <div class="avail-card">
                    <div>
                        <div class="avail-title" onclick="openCourse('${safeName}')">${c.name}</div>
                        <div class="avail-desc">Загвар хичээлийн тайлбар тодорхойлолт • ${c.credit} кредит</div>
                        <div style="font-size:13px;"><b>Тэнхим / Department:</b> <span style="color:var(--green-main);">${c.department}</span></div>
                    </div>
                    <button class="btn-primary" style="background:${already ? '#94a3b8' : 'var(--green-main)'}; white-space:nowrap;"
                        onclick="enrollCourse('${safeName}', ${c.credit})">
                        ${already ? t('enrolled') : t('enroll')}
                    </button>
                </div>`;
                }).join('')}
        `;
    }
    else if (tab === 'dashboard') {
        html = `
            <h1 class="page-heading">${t('nav_dashboard')}</h1>
            <div class="greeting">${t('hi')}, ${currentUser.firstName}! 👋</div>

            <!-- Timeline -->
            <div class="moodle-card">
                <div class="card-title">${t('timeline')}</div>
                <div class="controls-row">
                    <select class="m-select">
                        <option>${t('next_30')}</option>
                        <option>${t('next_7')}</option>
                        <option>${t('all')}</option>
                    </select>
                    <select class="m-select">
                        <option>${t('sort_dates')}</option>
                        <option>${t('sort_courses')}</option>
                    </select>
                    <input class="m-input" type="text" placeholder="${t('search_activity')}">
                </div>
                <div style="padding:18px 0 6px; border-top:1px solid #f1f5f9;">
                    ${calendarEvents.length > 0 ?
                calendarEvents.slice(0, 5).map(ev => `
                            <div style="padding:10px 14px; background:#f8fafc; border-radius:8px; margin-bottom:8px; display:flex; justify-content:space-between; align-items:center; cursor:pointer;"
                                 onclick="openEventDetail(${ev.id})">
                                <span>📌 <b>${ev.title}</b> <small style="color:var(--muted);">(${ev.courseName || 'Ерөнхий'})</small></span>
                                <span style="color:var(--green-main); font-weight:600; font-size:13px;">${ev.date} ${ev.time}</span>
                            </div>
                        `).join('') :
                `<div style="text-align:center; padding:24px 0; color:var(--muted);">
                            <div style="font-size:34px; opacity:0.4; margin-bottom:8px;">📋</div>
                            <div>${t('no_activities')}</div>
                        </div>`
            }
                </div>
            </div>

            <!-- Calendar -->
            <div class="moodle-card">
                <div class="card-title">${t('calendar')}</div>
                ${buildCalendarHtml()}
            </div>

            <!-- Course Overview -->
            <div class="moodle-card">
                <div class="card-title">${t('course_overview')}</div>
                ${buildCourseOverviewHtml()}
            </div>

            <!-- Feedback -->
            <div class="moodle-card">
                <div class="card-title">${t('feedback_title')}</div>
                <p style="font-size:14px; color:#334155;">${t('feedback_desc')}</p>
            </div>
        `;
    }
    else if (tab === 'courses') {
        html = `
            <h1 class="page-heading">${t('nav_courses')}</h1>
            <div class="moodle-card">
                <div class="card-title">${t('course_overview')}</div>
                ${buildCourseOverviewHtml()}
            </div>
        `;
    }
    else if (tab === 'tuition') {
        wrapper.innerHTML = `<div class="lms-container">${renderStudentTuitionHtml()}</div>`;
        return;
    }
    else if (tab === 'grades') {
        const avg = studentData.grades.length > 0
            ? (studentData.grades.reduce((s, g) => s + g.score, 0) / studentData.grades.length).toFixed(1)
            : '0.0';

        html = `
            <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:18px;">
    <h1 class="page-heading" style="margin:0;">📊 Дүнгийн жагсаалт</h1>
    <button class="btn-primary" onclick="printStudentTranscript()">🖨️ Дүнгийн хуудас хэвлэх</button>
</div>
            <div class="moodle-card">
                <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:16px;">
                    <div class="card-title" style="margin:0;">Улирлын дүнгийн жагсаалт</div>
                    <span class="grade-pill">Дундаж: ${avg}</span>
                </div>
                ${studentData.grades.length === 0 ?
                `<p style="color:var(--muted); text-align:center; padding:30px;">Одоогоор оруулсан дүн байхгүй байна.</p>` :
                `<table class="grades-table">
                        <thead>
                            <tr><th>Хичээлийн нэр</th><th>Кредит</th><th>Оноо</th><th>Үсгэн үнэлгээ</th></tr>
                        </thead>
                        <tbody>
                            ${studentData.grades.map(g => `
                                <tr>
                                    <td style="color:var(--green-main); font-weight:600; cursor:pointer;" onclick="openCourse('${g.name.replace(/'/g, "\\'")}')">${g.name}</td>
                                    <td>${g.credit}</td>
                                    <td><b>${g.score}</b> / 100</td>
                                    <td><span class="grade-pill">${g.letter}</span></td>
                                </tr>
                            `).join('')}
                        </tbody>
                    </table>`
            }
            </div>
        `;
    }
    else if (tab === 'profile') {
        const nowStr = new Date().toLocaleString(currentLang === 'mn' ? 'mn-MN' : 'en-US');
        html = `
            <div class="profile-header">
                <div class="avatar profile-avatar-lg">${getInitials(currentUser.lastName, currentUser.firstName)}</div>
                <div>
                    <h1 style="font-size:24px; color:#0f172a;">${currentUser.firstName} ${currentUser.lastName}</h1>
                    <button class="m-select" style="margin-top:6px; cursor:pointer;" onclick="toggleChatDrawer(true)">💬 Message</button>
                </div>
            </div>

            <div class="profile-grid">
                <div>
                    <div class="moodle-card">
                        <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:14px;">
                            <div class="card-title" style="margin:0;">User details</div>
                            <span class="profile-link" style="margin:0;" onclick="showToast('Профайл засах эрхийг админ удирдана.')">Edit profile</span>
                        </div>
                        <div style="margin-bottom:12px;">
                            <b style="font-size:13.5px;">Email address</b>
                            <div style="color:var(--green-main); font-size:14px;">${currentUser.email}</div>
                            <small style="color:var(--muted);">(Visible to other course participants)</small>
                        </div>
                        <div style="margin-bottom:12px;">
                            <b style="font-size:13.5px;">Student ID / Major</b>
                            <div style="font-size:14px;">${currentUser.id} • ${currentUser.major}</div>
                        </div>
                        <div>
                            <b style="font-size:13.5px;">Timezone</b>
                            <div style="font-size:14px;">Asia/Ulaanbaatar</div>
                        </div>
                    </div>

                    <div class="moodle-card">
                        <div class="card-title">Privacy and policies</div>
                        <span class="profile-link" onclick="showToast('Мэдээллийн нууцлалын бодлого идэвхтэй.')">Data retention summary</span>
                    </div>

                    <div class="moodle-card">
                        <div class="card-title">Course details</div>
                        <b style="font-size:13.5px;">Course profiles</b>
                        ${studentData.enrolled.length === 0 ? '<div style="color:var(--muted); font-size:13.5px; margin-top:6px;">Сонгосон хичээл байхгүй</div>' :
                studentData.enrolled.map(c => `<span class="profile-link" onclick="openCourse('${c.name.replace(/'/g, "\\'")}')">${c.name}</span>`).join('')}
                    </div>
                </div>

                <div>
                    <div class="moodle-card">
                        <div class="card-title">Reports</div>
                        <span class="profile-link" onclick="showToast('Идэвхтэй сесс: Desktop WebView2 (Windows)')">Browser sessions</span>
                        <span class="profile-link" onclick="switchLmsTab('grades')">Grades overview</span>
                    </div>

                    <div class="moodle-card">
                        <div class="card-title">Login activity</div>
                        <div style="margin-bottom:12px;">
                            <b style="font-size:13.5px;">First access to site</b>
                            <div style="font-size:13.5px; color:#334155;">Tuesday, 17 November 2026, 8:39 AM</div>
                        </div>
                        <div>
                            <b style="font-size:13.5px;">Last access to site</b>
                            <div style="font-size:13.5px; color:#334155;">${nowStr}</div>
                        </div>
                    </div>

                    <div class="moodle-card">
                        <div class="card-title">Mobile app</div>
                        <p style="font-size:13.5px; color:var(--muted);">QR code for mobile app access (KOSEN LMS)</p>
                    </div>
                </div>
            </div>
        `;
    }
    else if (tab === 'calendar') {
        html = `
            <h1 class="page-heading">${t('calendar')}</h1>
            <div class="moodle-card">${buildCalendarHtml()}</div>
        `;
    }

    wrapper.innerHTML = `<div class="lms-container">${html}</div>`;
}

function buildCourseOverviewHtml() {
    const patterns = ['pat-pink', 'pat-purple', 'pat-teal', 'pat-amber'];
    let list = [...studentData.enrolled].filter(c => c.name.toLowerCase().includes(courseSearchText.toLowerCase()));
    if (courseSortMode === 'name') list.sort((a, b) => a.name.localeCompare(b.name));

    return `
        <div class="controls-row">
            <select class="m-select"><option>${t('all')}</option></select>
            <input class="m-input" type="text" placeholder="${t('search_courses')}" value="${courseSearchText}" oninput="courseSearchText=this.value; renderCurrentLmsView();">
            <select class="m-select" onchange="courseSortMode=this.value; renderCurrentLmsView();">
                <option value="name">${t('sort_name')}</option>
            </select>
            <select class="m-select" onchange="courseViewMode=this.value; renderCurrentLmsView();">
                <option value="card" ${courseViewMode === 'card' ? 'selected' : ''}>${t('card_view')}</option>
                <option value="list" ${courseViewMode === 'list' ? 'selected' : ''}>${t('list_view')}</option>
            </select>
        </div>
        ${list.length === 0 ?
            `<div style="text-align:center; padding:28px; color:var(--muted);">
                Сонгосон хичээл алга. <a href="#" onclick="switchLmsTab('home'); return false;" style="color:var(--green-main); font-weight:600;">${t('nav_home')} цэснээс хичээл сонгоно уу.</a>
            </div>` :
            `<div class="course-grid ${courseViewMode === 'list' ? 'list-mode' : ''}">
                ${list.map((c, idx) => {
                const safeName = c.name.replace(/'/g, "\\'");
                return `
                    <div class="course-card">
                        <div class="course-banner ${patterns[idx % patterns.length]}" onclick="openCourse('${safeName}')"></div>
                        <div class="course-body">
                            <div>
                                <div class="course-name" onclick="openCourse('${safeName}')">${c.name}</div>
                                <div class="course-sub">Блок 4 • ${c.credit} кредит</div>
                            </div>
                            <div class="course-footer">
                                <span>0% complete</span>
                                <button class="dots-btn" title="Хичээл хасах" onclick="confirmUnenroll('${safeName}')">⋮</button>
                            </div>
                        </div>
                    </div>`;
            }).join('')}
            </div>`
        }
    `;
}

function enrollCourse(courseName, credit) {
    postToCSharp({ action: 'enrollCourse', studentId: currentUser.id, courseName, credit });
}

function confirmUnenroll(courseName) {
    if (confirm(`"${courseName}" хичээлийг сонголтоос хасах уу?`)) {
        postToCSharp({ action: 'unenrollCourse', studentId: currentUser.id, courseName });
    }
}

// ================= ОЮУТНЫ ТӨЛБӨР БА ХЭВЛЭХ =================
function renderStudentTuitionHtml() {
    const list = studentData.payments || [];
    const totalNet = list.reduce((s, p) => s + Math.max(0, p.totalFee - p.discountAmount), 0);
    const totalPaid = list.reduce((s, p) => s + p.paidAmount, 0);
    const remain = Math.max(0, totalNet - totalPaid);

    return `
        <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:18px;">
            <h1 class="page-heading" style="margin:0;">💳 Сургалтын төлбөрийн мэдээлэл</h1>
            <button class="btn-primary" onclick="printStudentTranscript()">🖨️ Дүнгийн тодорхойлолт хэвлэх</button>
        </div>

        <div style="display:grid; grid-template-columns:repeat(auto-fit, minmax(200px, 1fr)); gap:14px; margin-bottom:20px;">
            <div class="moodle-card" style="margin:0; text-align:center;">
                <div style="font-size:24px; font-weight:800; color:#0f172a;">${totalNet.toLocaleString()}₮</div>
                <div style="font-size:13px; color:var(--muted);">Нийт төлөх дүн</div>
            </div>
            <div class="moodle-card" style="margin:0; text-align:center;">
                <div style="font-size:24px; font-weight:800; color:#16a34a;">${totalPaid.toLocaleString()}₮</div>
                <div style="font-size:13px; color:var(--muted);">Төлсөн дүн</div>
            </div>
            <div class="moodle-card" style="margin:0; text-align:center;">
                <div style="font-size:24px; font-weight:800; color:${remain > 0 ? '#dc2626' : '#16a34a'};">${remain.toLocaleString()}₮</div>
                <div style="font-size:13px; color:var(--muted);">Үлдэгдэл төлбөр</div>
            </div>
        </div>

        <div class="moodle-card">
            <div class="card-title">Төлбөрийн нэхэмжлэхүүд (${list.length})</div>
            ${list.length === 0 ? `<p style="color:var(--muted); text-align:center; padding:24px;">Одоогоор төлбөрийн нэхэмжлэх бүртгэгдээгүй байна.</p>` : `
            <table class="grades-table">
                <thead><tr><th>Улирал</th><th>Үндсэн төлбөр</th><th>Хөнгөлөлт</th><th>Төлсөн</th><th>Төлөв</th><th>Баримт</th></tr></thead>
                <tbody>
                    ${list.map((p, idx) => `
                        <tr>
                            <td><b>${p.semester}</b></td>
                            <td>${p.totalFee.toLocaleString()}₮</td>
                            <td style="color:#16a34a;">-${p.discountAmount.toLocaleString()}₮</td>
                            <td><b>${p.paidAmount.toLocaleString()}₮</b></td>
                            <td><span class="grade-pill">${p.status}</span></td>
                            <td><button class="m-select" style="cursor:pointer;" onclick="printStudentReceipt(${idx})">🖨️ Хэвлэх</button></td>
                        </tr>
                    `).join('')}
                </tbody>
            </table>`}
        </div>
    `;
}

function printStudentReceipt(idx) {
    const p = studentData.payments[idx];
    if (!p) return;
    const net = Math.max(0, p.totalFee - p.discountAmount);
    const remain = Math.max(0, net - p.paidAmount);

    printDocument('Сургалтын төлбөрийн баримт', `
        <p><b>Оюутан:</b> ${currentUser.lastName} ${currentUser.firstName} (${currentUser.id})</p>
        <p><b>Мэргэжил:</b> ${currentUser.major}</p>
        <p><b>Улирал:</b> ${p.semester}</p>
        <table class="grades-table" style="margin-top:14px;">
            <tr><td>Үндсэн төлбөр</td><td><b>${p.totalFee.toLocaleString()}₮</b></td></tr>
            <tr><td>Хөнгөлөлт</td><td>-${p.discountAmount.toLocaleString()}₮</td></tr>
            <tr><td>Нийт төлөх дүн</td><td><b>${net.toLocaleString()}₮</b></td></tr>
            <tr><td>Төлсөн дүн</td><td><b>${p.paidAmount.toLocaleString()}₮</b></td></tr>
            <tr><td>Үлдэгдэл</td><td><b>${remain.toLocaleString()}₮</b></td></tr>
            <tr><td>Төлөв</td><td><b>${p.status}</b></td></tr>
        </table>
    `);
}

function printStudentTranscript() {
    const grades = studentData.grades || [];
    const totalCr = grades.reduce((s, g) => s + g.credit, 0);
    const avgScore = grades.length ? (grades.reduce((s, g) => s + g.score, 0) / grades.length).toFixed(1) : '0.0';

    printDocument('Сургалтын дүнгийн тодорхойлолт', `
        <p><b>Овог, Нэр:</b> ${currentUser.lastName} ${currentUser.firstName}</p>
        <p><b>Оюутны код:</b> ${currentUser.id}   |   <b>Мэргэжил:</b> ${currentUser.major}</p>
        <table class="grades-table" style="margin-top:14px;">
            <thead><tr><th>№</th><th>Хичээлийн нэр</th><th>Кредит</th><th>Оноо</th><th>Үнэлгээ</th></tr></thead>
            <tbody>
                ${grades.length === 0
            ? `<tr><td colspan="5" style="text-align:center;">Дүн бүртгэгдээгүй байна.</td></tr>`
            : grades.map((g, i) => `<tr><td>${i + 1}</td><td>${g.name}</td><td>${g.credit}</td><td>${g.score}</td><td><b>${g.letter}</b></td></tr>`).join('')}
            </tbody>
        </table>
        <p style="margin-top:14px; text-align:right;"><b>Нийт кредит:</b> ${totalCr}   |   <b>Дундаж оноо:</b> ${avgScore}</p>
    `);
}