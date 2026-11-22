let activeCourseTab = 'course';
let collapsedModules = {};
let courseParticipantsList = [];
let courseGradesList = [];
let partSearch = '';
let partLetter = 'All';

function openCourse(courseName, highlightTitle = null) {
    closeMenus();
    activeCourseName = courseName;
    highlightActivityTitle = highlightTitle;
    activeCourseTab = 'course';
    partSearch = '';
    partLetter = 'All';
    currentLmsTab = 'courseDetail';

    document.querySelectorAll('.lms-link').forEach(l => l.classList.remove('active'));
    document.getElementById('nav-courses')?.classList.add('active');

    // MySQL-ээс тухайн хичээлийн модулиуд болон оролцогчдыг татах
    postToCSharp({ action: 'getCourseMaterials', courseName });
    postToCSharp({ action: 'getCourseParticipants', courseName });
    renderCourseDetailPage(courseName, highlightTitle);
}

function onCourseParticipantsReceived(participants, courseGrades) {
    courseParticipantsList = participants || [];
    courseGradesList = courseGrades || [];
    if (currentLmsTab === 'courseDetail' && (activeCourseTab === 'participants' || activeCourseTab === 'grades')) {
        renderCourseDetailPage(activeCourseName, highlightActivityTitle);
    }
}

function renderCourseDetailPage(courseName, highlightTitle) {
    const wrapper = document.getElementById('lms-main-wrapper');
    if (!wrapper) return;

    // Материалуудыг модулиар нь бүлэглэх
    const grouped = {};
    courseMaterials.forEach(m => {
        if (!grouped[m.moduleTitle]) grouped[m.moduleTitle] = [];
        grouped[m.moduleTitle].push(m);
    });
    const moduleNames = Object.keys(grouped);
    const iconMap = { lecture: '📄', lab: '💻', quiz: '✅', seminar: '📘' };

    const tabs = [
        { key: 'course', label: 'Course' },
        { key: 'participants', label: 'Participants' },
        { key: 'grades', label: 'Grades' },
        { key: 'activities', label: 'Activities' },
        { key: 'competencies', label: 'Competencies' }
    ];

    wrapper.innerHTML = `
        <div class="course-layout">
            <!-- ЗҮҮН ТАЛЫН МОДУЛИЙН ЦЭС -->
            <aside class="course-sidebar">
                <div class="cs-title">${courseName}</div>
                <div style="font-size:12.5px; color:var(--green-main); cursor:pointer; margin-bottom:12px; font-weight:600;" onclick="toggleAllModules()">
                    ⇕ Collapse / Expand all
                </div>
                <div class="cs-mod-header" onclick="switchCourseTab('course')">▾ Хичээлийн ерөнхий мэдээлэл</div>
                ${moduleNames.length === 0 ? `<div class="cs-item" style="color:var(--muted);">Одоогоор модуль алга</div>` :
            moduleNames.map(mod => `
                    <div class="cs-mod-header" onclick="toggleModule('${mod.replace(/'/g, "\\'")}')">
                        ${collapsedModules[mod] ? '▸' : '▾'} ${mod}
                    </div>
                    ${collapsedModules[mod] ? '' : grouped[mod].map(item => `
                        <div class="cs-item" onclick="scrollToMaterial('${item.itemTitle.replace(/'/g, "\\'")}')">
                            ${iconMap[item.itemType] || '📄'} ${item.itemTitle}
                        </div>
                    `).join('')}
                `).join('')}
            </aside>

            <!-- БАРУУН ТАЛЫН ҮНДСЭН ХИЧЭЭЛИЙН ХЭСЭГ -->
            <section class="course-content">
                <div style="display:flex; justify-content:space-between; align-items:center;">
                    <h1 class="page-heading" style="margin:0;">${courseName}</h1>
                    <button class="m-select" style="cursor:pointer;" onclick="switchLmsTab('courses')">← Миний хичээлүүд</button>
                </div>

                <!-- MOODLE ЗАГВАРЫН 5 ТАБ -->
                <div class="course-subnav">
                    ${tabs.map(tb => `
                        <div class="course-subnav-item ${activeCourseTab === tb.key ? 'active' : ''}"
                             onclick="switchCourseTab('${tb.key}')">
                            ${activeCourseTab === tb.key ? '• ' : ''}${tb.label}
                        </div>
                    `).join('')}
                </div>

                <div id="course-tab-body">
                    ${buildCourseTabBody(courseName, grouped, moduleNames, highlightTitle, iconMap)}
                </div>
            </section>
        </div>
    `;
}

function getFilteredParticipants() {
    return courseParticipantsList.filter(p => {
        const full = `${p.lastName} ${p.firstName}`.toLowerCase();
        const matchSearch = full.includes(partSearch.toLowerCase()) || p.id.toLowerCase().includes(partSearch.toLowerCase());
        const matchLetter = partLetter === 'All'
            || (p.firstName || '').toUpperCase().startsWith(partLetter)
            || (p.lastName || '').toUpperCase().startsWith(partLetter);
        return matchSearch && matchLetter;
    });
}

function renderParticipantsRows() {
    const filtered = getFilteredParticipants();
    const countEl = document.getElementById('part-count-label');
    if (countEl) countEl.innerText = `${filtered.length} оролцогч олдлоо (Танхим)`;

    const tbody = document.getElementById('part-table-body');
    if (!tbody) return;

    tbody.innerHTML = filtered.length === 0
        ? `<tr><td colspan="4" style="text-align:center; color:var(--muted);">Оролцогч олдсонгүй.</td></tr>`
        : filtered.map(p => `
            <tr>
                <td style="display:flex; align-items:center; gap:10px;">
                    <div class="avatar-circle" style="width:32px; height:32px; font-size:12px;">${getInitials(p.lastName, p.firstName)}</div>
                    <b style="color:var(--green-main);">${p.lastName} ${p.firstName}</b>
                </td>
                <td>${p.id}</td>
                <td>Student</td>
                <td><span class="grade-pill">Танхим</span></td>
            </tr>
        `).join('');
}

function buildCourseTabBody(courseName, grouped, moduleNames, highlightTitle, iconMap) {
    // 1. PARTICIPANTS (Оролцогчид)
    if (activeCourseTab === 'participants') {
        const letters = ['All', 'А', 'Б', 'Г', 'Д', 'Ж', 'З', 'М', 'Н', 'О', 'С', 'Т', 'У', 'Х', 'Ц', 'Э', 'A', 'B', 'C', 'D', 'E', 'K', 'M', 'N', 'S', 'T'];
        const filtered = getFilteredParticipants();

        return `
            <div class="moodle-card">
                <div style="display:flex; justify-content:space-between; align-items:center; flex-wrap:wrap; gap:10px; margin-bottom:14px;">
                    <b id="part-count-label">${filtered.length} оролцогч олдлоо (Танхим)</b>
                    <input class="m-input" type="text" placeholder="Нэр эсвэл кодоор хайх..." value="${partSearch}"
                           oninput="partSearch=this.value; renderParticipantsRows();">
                </div>

                <div class="alpha-filter">
                    ${letters.map(l => `
                        <button class="alpha-btn ${partLetter === l ? 'active' : ''}"
                                onclick="partLetter='${l}'; renderCourseDetailPage(activeCourseName, highlightActivityTitle);">${l}</button>
                    `).join('')}
                </div>

                <table class="grades-table" style="margin-top:14px;">
                    <thead><tr><th>Овог, Нэр</th><th>Оюутны код</th><th>Эрх (Roles)</th><th>Бүлэг (Groups)</th></tr></thead>
                    <tbody id="part-table-body">
                        ${filtered.length === 0
                ? `<tr><td colspan="4" style="text-align:center; color:var(--muted);">Оролцогч олдсонгүй.</td></tr>`
                : filtered.map(p => `
                                <tr>
                                    <td style="display:flex; align-items:center; gap:10px;">
                                        <div class="avatar-circle" style="width:32px; height:32px; font-size:12px;">${getInitials(p.lastName, p.firstName)}</div>
                                        <b style="color:var(--green-main);">${p.lastName} ${p.firstName}</b>
                                    </td>
                                    <td>${p.id}</td>
                                    <td>Student</td>
                                    <td><span class="grade-pill">Танхим</span></td>
                                </tr>
                            `).join('')}
                    </tbody>
                </table>
            </div>
        `;
    }

    // 2. GRADES (Энэ хичээлийн дүн)
    if (activeCourseTab === 'grades') {
        const list = currentUser.role === 'student'
            ? courseGradesList.filter(g => g.studentId === currentUser.id)
            : courseGradesList;

        // Хэрэв MySQL-ээс ирээгүй бол оюутны өөрийн studentData.grades-ээс шалгах
        const fallback = (list.length === 0 && currentUser.role === 'student')
            ? studentData.grades.filter(x => x.name === courseName).map(x => ({
                studentId: currentUser.id,
                studentName: `${currentUser.lastName} ${currentUser.firstName}`,
                score: x.score,
                letter: x.letter
            }))
            : list;

        return `
            <div class="moodle-card">
                <div class="card-title">📊 "${courseName}" хичээлийн үнэлгээ</div>
                ${fallback.length === 0
                ? `<p style="color:var(--muted); text-align:center; padding:20px;">Энэ хичээлд одоогоор дүн бүртгэгдээгүй байна.</p>`
                : `<table class="grades-table">
                        <thead><tr><th>Оюутан</th><th>Оноо (0-100)</th><th>Үсгэн үнэлгээ</th></tr></thead>
                        <tbody>
                            ${fallback.map(g => `<tr><td><b>${g.studentName || g.studentId}</b></td><td>${g.score}</td><td><span class="grade-pill">${g.letter}</span></td></tr>`).join('')}
                        </tbody>
                    </table>`}
            </div>
        `;
    }

    // 3. ACTIVITIES (Бүх үйл ажиллагаа / Даалгаврууд)
    if (activeCourseTab === 'activities') {
        return `
            <div class="moodle-card">
                <div class="card-title">📝 Хичээлийн үйл ажиллагаа & Материалууд (${courseMaterials.length})</div>
                ${courseMaterials.length === 0
                ? `<p style="color:var(--muted); text-align:center; padding:20px;">Одоогоор даалгавар/материал ороогүй байна.</p>`
                : `<table class="grades-table">
                        <thead><tr><th>Төрөл</th><th>Гарчиг</th><th>Модуль</th><th>Тайлбар</th></tr></thead>
                        <tbody>
                            ${courseMaterials.map(m => `
                                <tr>
                                    <td><span class="grade-pill">${iconMap[m.itemType] || '📄'} ${m.itemType || 'материал'}</span></td>
                                    <td><b>${m.itemTitle}</b></td>
                                    <td>${m.moduleTitle}</td>
                                    <td>${m.description || '—'}</td>
                                </tr>
                            `).join('')}
                        </tbody>
                    </table>`}
            </div>
        `;
    }

    // 4. COMPETENCIES (Эзэмших чадамжууд)
    if (activeCourseTab === 'competencies') {
        const comps = [
            { code: 'COMP-01', title: 'Онолын суурь мэдлэг ба инженерийн тооцоолол хийх чадвар', status: 'Хэрэгжиж буй' },
            { code: 'COMP-02', title: 'Лабораторийн болон бие даалтын ажлыг бие даан гүйцэтгэх', status: 'Хэрэгжиж буй' },
            { code: 'COMP-03', title: 'Багаар ажиллах, тайлан хамгаалах ба техникийн баримт бичиг боловсруулах', status: 'Төлөвлөгдсөн' }
        ];
        return `
            <div class="moodle-card">
                <div class="card-title">🎯 Хичээлээр эзэмших чадамжууд (Competencies)</div>
                <table class="grades-table">
                    <thead><tr><th>Код</th><th>Чадамжийн тодорхойлолт</th><th>Төлөв</th></tr></thead>
                    <tbody>
                        ${comps.map(c => `<tr><td><b>${c.code}</b></td><td>${c.title}</td><td><span class="grade-pill">${c.status}</span></td></tr>`).join('')}
                    </tbody>
                </table>
            </div>
        `;
    }

    // 5. DEFAULT: COURSE TAB (Модулиуд ба материалууд)
    return `
        <div class="module-box">
            <div class="module-box-header">▾ Хичээлийн ерөнхий мэдээлэл, зар</div>
            <div class="module-box-body">
                <p style="font-size:14px; color:#334155; line-height:1.6;">
                    <b>Хичээлийн тухай:</b> ${courseName} хичээлийн лекц, семинар, лабораторийн удирдамж болон явцын шалгалтын материалууд энэ хэсэгт байрлана.
                </p>
            </div>
        </div>

        ${moduleNames.length === 0 ?
            `<div class="moodle-card" style="text-align:center; color:var(--muted); padding:32px;">
                Багш энэ хичээлд одоогоор материал оруулаагүй байна.
            </div>` :
            moduleNames.map(mod => `
                <div class="module-box">
                    <div class="module-box-header" onclick="toggleModule('${mod.replace(/'/g, "\\'")}')">
                        <span>${collapsedModules[mod] ? '▸' : '▾'} ${mod}</span>
                    </div>
                    ${collapsedModules[mod] ? '' : `
                    <div class="module-box-body">
                        ${grouped[mod].map(item => {
                const isHl = highlightTitle && item.itemTitle.toLowerCase().includes(highlightTitle.toLowerCase());
                return `
                            <div class="material-row ${isHl ? 'highlight' : ''}" id="mat-${item.id}">
                                <div>
                                    <div style="font-weight:600; font-size:14.5px; color:var(--green-dark);">
                                        ${iconMap[item.itemType] || '📄'} ${item.itemTitle}
                                    </div>
                                    <div style="font-size:13px; color:#475569; margin-top:4px;">${item.description || ''}</div>
                                </div>
                                ${item.fileName ? `<button class="btn-primary" style="padding:7px 14px; font-size:12.5px;" onclick="showToast('Файл нээж байна: ${item.fileName}')">📥 ${item.fileName}</button>` :
                        `<button class="m-select" style="cursor:pointer;" onclick="showToast('${item.itemTitle} нээгдлээ')">Нээх</button>`}
                            </div>`;
            }).join('')}
                    </div>`}
                </div>
            `).join('')
        }
    `;
}

function switchCourseTab(tab) {
    activeCourseTab = tab;
    if (tab === 'participants' || tab === 'grades') {
        postToCSharp({ action: 'getCourseParticipants', courseName: activeCourseName });
    }
    renderCourseDetailPage(activeCourseName, highlightActivityTitle);
}

function toggleModule(modName) {
    collapsedModules[modName] = !collapsedModules[modName];
    renderCourseDetailPage(activeCourseName, highlightActivityTitle);
}

function toggleAllModules() {
    const anyOpen = courseMaterials.some(m => !collapsedModules[m.moduleTitle]);
    courseMaterials.forEach(m => collapsedModules[m.moduleTitle] = anyOpen);
    renderCourseDetailPage(activeCourseName, highlightActivityTitle);
}

function scrollToMaterial(title) {
    highlightActivityTitle = title;
    activeCourseTab = 'course';
    renderCourseDetailPage(activeCourseName, title);
}