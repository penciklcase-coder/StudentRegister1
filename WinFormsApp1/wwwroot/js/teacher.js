let teacherTab = 'overview';
let teacherStudentSearch = '';
let selectedMaterialCourse = '';

function renderTeacherPortal() {
    const wrapper = document.getElementById('lms-main-wrapper');

    // Дээд талын оюутны цэсийг багшийн горимд нууж цэвэрхэн болгох
    const navLinks = document.getElementById('lms-nav-links');
    if (navLinks) {
        navLinks.innerHTML = `
            <div class="lms-link ${teacherTab === 'overview' ? 'active' : ''}" onclick="switchTeacherTab('overview')">📊 Хянах самбар</div>
            <div class="lms-link ${teacherTab === 'courses' ? 'active' : ''}" onclick="switchTeacherTab('courses')">📚 Хичээл & Материал</div>
            <div class="lms-link ${teacherTab === 'grades' ? 'active' : ''}" onclick="switchTeacherTab('grades')">📝 Дүн оруулах</div>
            <div class="lms-link ${teacherTab === 'students' ? 'active' : ''}" onclick="switchTeacherTab('students')">👨‍🎓 Оюутнууд</div>
        `;
    }

    let html = '';

    if (teacherTab === 'overview') {
        html = `
            <h1 class="page-heading">Багшийн удирдлагын самбар</h1>
            <div class="greeting">Сайн байна уу, ${currentUser.firstName} багш аа! 👋</div>

            <div style="display:grid; grid-template-columns:repeat(auto-fit, minmax(180px, 1fr)); gap:16px; margin-bottom:24px;">
                <div class="moodle-card" style="margin:0; text-align:center; cursor:pointer;" onclick="switchTeacherTab('students')">
                    <div style="font-size:28px; font-weight:800; color:var(--green-main);">${teacherData.students.length}</div>
                    <div style="font-size:13px; color:var(--muted); margin-top:4px;">Нийт оюутан</div>
                </div>
                <div class="moodle-card" style="margin:0; text-align:center; cursor:pointer;" onclick="switchTeacherTab('courses')">
                    <div style="font-size:28px; font-weight:800; color:#0284c7;">${teacherData.courses.length}</div>
                    <div style="font-size:13px; color:var(--muted); margin-top:4px;">Идэвхтэй хичээл</div>
                </div>
                <div class="moodle-card" style="margin:0; text-align:center; cursor:pointer;" onclick="switchTeacherTab('courses')">
                    <div style="font-size:28px; font-weight:800; color:#7c3aed;">${teacherData.materials.length}</div>
                    <div style="font-size:13px; color:var(--muted); margin-top:4px;">Оруулсан материал</div>
                </div>
                <div class="moodle-card" style="margin:0; text-align:center; cursor:pointer;" onclick="switchTeacherTab('grades')">
                    <div style="font-size:28px; font-weight:800; color:#d97706;">${teacherData.grades.length}</div>
                    <div style="font-size:13px; color:var(--muted); margin-top:4px;">Нийт дүн</div>
                </div>
            </div>

            <div class="moodle-card">
                <div class="card-title">📅 Нийт оюутны календарьт тов / шалгалт нэмэх</div>
                <div class="input-row">
                    <div class="input-group">
                        <label>Хичээл:</label>
                        <select id="t-ev-course">
                            ${teacherData.courses.map(c => `<option value="${c.name}">${c.name}</option>`).join('')}
                        </select>
                    </div>
                    <div class="input-group">
                        <label>Үйл явдлын нэр (Жнь: Модуль 2 явц):</label>
                        <input type="text" id="t-ev-title" placeholder="Шалгалт эсвэл даалгаврын нэр...">
                    </div>
                </div>
                <div class="input-row">
                    <div class="input-group">
                        <label>Огноо:</label>
                        <input type="date" id="t-ev-date" value="${new Date().toISOString().slice(0, 10)}">
                    </div>
                    <div class="input-group">
                        <label>Цаг:</label>
                        <input type="time" id="t-ev-time" value="09:00">
                    </div>
                </div>
                <button class="btn-primary" onclick="teacherAddClassEvent()">+ Календарьт нийтлэх</button>
            </div>
        `;
    }
    else if (teacherTab === 'courses') {
        if (!selectedMaterialCourse && teacherData.courses.length > 0) {
            selectedMaterialCourse = teacherData.courses[0].name;
        }
        const filteredMats = teacherData.materials.filter(m => !selectedMaterialCourse || m.courseName === selectedMaterialCourse);

        html = `
            <h1 class="page-heading">Хичээл & Сургалтын материал</h1>

            <!-- 1. Шинэ хичээл нэмэх -->
            <div class="moodle-card">
                <div class="card-title">➕ Шинэ хичээл үүсгэх</div>
                <div class="input-row">
                    <div class="input-group" style="flex:2;">
                        <label>Хичээлийн нэр:</label>
                        <input type="text" id="t-course-name" placeholder="Жнь: Өгөгдлийн сан DB302">
                    </div>
                    <div class="input-group">
                        <label>Кредит:</label>
                        <input type="number" id="t-course-credit" value="3" min="1" max="6">
                    </div>
                    <div class="input-group">
                        <label>Тэнхим:</label>
                        <input type="text" id="t-course-dept" value="${currentUser.major || 'Мэдээллийн технологи'}">
                    </div>
                </div>
                <button class="btn-primary" onclick="teacherAddCourse()">+ Хичээл нэмэх</button>
            </div>

            <!-- 2. Хичээлд материал / модуль нэмэх -->
            <div class="moodle-card">
                <div class="card-title">📄 Хичээлийн материал, лекц, лаборатори оруулах</div>
                <div class="input-row">
                    <div class="input-group">
                        <label>Хичээл сонгох:</label>
                        <select id="t-mat-course" onchange="selectedMaterialCourse=this.value; renderTeacherPortal();">
                            ${teacherData.courses.map(c => `<option value="${c.name}" ${c.name === selectedMaterialCourse ? 'selected' : ''}>${c.name}</option>`).join('')}
                        </select>
                    </div>
                    <div class="input-group">
                        <label>Модулийн нэр:</label>
                        <input type="text" id="t-mat-module" placeholder="Жнь: Модуль 1. Үндсэн ойлголт">
                    </div>
                </div>
                <div class="input-row">
                    <div class="input-group">
                        <label>Гарчиг (Лекц / Лаб):</label>
                        <input type="text" id="t-mat-title" placeholder="Жнь: Лекц №2: SQL хүсэлт">
                    </div>
                    <div class="input-group">
                        <label>Төрөл:</label>
                        <select id="t-mat-type">
                            <option value="lecture">📄 Лекц</option>
                            <option value="lab">💻 Лаборатор</option>
                            <option value="seminar">📘 Семинар</option>
                            <option value="quiz">✅ Явцын сорил</option>
                        </select>
                    </div>
                    <div class="input-group">
                        <label>Файлын нэр (заавал биш):</label>
                        <input type="text" id="t-mat-file" placeholder="Lecture_02.pdf">
                    </div>
                </div>
                <div class="input-group">
                    <label>Тайлбар:</label>
                    <input type="text" id="t-mat-desc" placeholder="Хичээлийн товч агуулга эсвэл даалгаврын заавар...">
                </div>
                <button class="btn-primary" onclick="teacherAddMaterial()">+ Материал байршуулах</button>

                <div style="margin-top:22px; border-top:1px solid var(--border); padding-top:16px;">
                    <div style="font-weight:600; margin-bottom:10px; font-size:14px;">"${selectedMaterialCourse}" хичээлийн материалууд (${filteredMats.length}):</div>
                    ${filteredMats.length === 0 ? `<p style="color:var(--muted); font-size:13.5px;">Энэ хичээлд одоогоор материал байхгүй байна.</p>` :
                filteredMats.map(m => `
                        <div class="material-row">
                            <div>
                                <div style="font-weight:600; font-size:14px;">${m.moduleTitle} ➔ ${m.itemTitle}</div>
                                <div style="font-size:12.5px; color:var(--muted);">${m.description || ''} ${m.fileName ? `• 📎 ${m.fileName}` : ''}</div>
                            </div>
                            <button class="m-select" style="color:#dc2626; cursor:pointer;" onclick="teacherDeleteMaterial(${m.id})">Устгах</button>
                        </div>
                    `).join('')}
                </div>
            </div>
        `;
    }
    else if (teacherTab === 'grades') {
        html = `
            <h1 class="page-heading">Оюутны дүн оруулах</h1>

            <div class="moodle-card">
                <div class="card-title">📝 Шинэ дүн бүртгэх</div>
                <div class="input-row">
                    <div class="input-group">
                        <label>Оюутан сонгох:</label>
                        <select id="t-grade-stu">
                            ${teacherData.students.map(s => `<option value="${s.id}">${s.id} - ${s.lastName} ${s.firstName}</option>`).join('')}
                        </select>
                    </div>
                    <div class="input-group">
                        <label>Хичээл сонгох:</label>
                        <select id="t-grade-course">
                            ${teacherData.courses.map(c => `<option value="${c.name}" data-credit="${c.credit}">${c.name} (${c.credit} кр)</option>`).join('')}
                        </select>
                    </div>
                    <div class="input-group" style="max-width:130px;">
                        <label>Оноо (0-100):</label>
                        <input type="number" id="t-grade-score" min="0" max="100" placeholder="95">
                    </div>
                </div>
                <button class="btn-primary" onclick="teacherSaveGrade()">+ Дүн хадгалах</button>
            </div>

            <div class="moodle-card">
                <div class="card-title">Оруулсан нийт дүнгийн жагсаалт (${teacherData.grades.length})</div>
                ${teacherData.grades.length === 0 ? `<p style="color:var(--muted); text-align:center; padding:20px;">Одоогоор дүн байхгүй байна.</p>` : `
                <table class="grades-table">
                    <thead>
                        <tr><th>Оюутны код</th><th>Хичээлийн нэр</th><th>Кредит</th><th>Оноо</th><th>Үнэлгээ</th><th>Үйлдэл</th></tr>
                    </thead>
                    <tbody>
                        ${teacherData.grades.map(g => `
                            <tr>
                                <td><b>${g.studentId}</b></td>
                                <td style="color:var(--green-main); font-weight:600;">${g.courseName}</td>
                                <td>${g.credit}</td>
                                <td><b>${g.score}</b></td>
                                <td><span class="grade-pill">${g.letter}</span></td>
                                <td><button class="m-select" style="color:#dc2626; cursor:pointer; padding:4px 10px;" onclick="teacherDeleteGrade('${g.studentId}', '${g.courseName.replace(/'/g, "\\'")}')">Устгах</button></td>
                            </tr>
                        `).join('')}
                    </tbody>
                </table>`}
            </div>
        `;
    }
    else if (teacherTab === 'students') {
        const filteredStu = teacherData.students.filter(s =>
            s.id.toLowerCase().includes(teacherStudentSearch.toLowerCase()) ||
            `${s.lastName} ${s.firstName}`.toLowerCase().includes(teacherStudentSearch.toLowerCase()) ||
            s.major.toLowerCase().includes(teacherStudentSearch.toLowerCase())
        );

        html = `
            <h1 class="page-heading">Оюутны жагсаалт</h1>
            <div class="moodle-card">
                <div class="controls-row">
                    <input class="m-input" type="text" placeholder="Оюутны код, нэр эсвэл мэргэжлээр хайх..."
                           value="${teacherStudentSearch}" oninput="teacherStudentSearch=this.value; renderTeacherPortal();">
                </div>
                <table class="grades-table">
                    <thead>
                        <tr><th>Код</th><th>Овог, Нэр</th><th>Мэргэжил</th><th>Имэйл</th><th>Холбогдох</th></tr>
                    </thead>
                    <tbody>
                        ${filteredStu.map(s => `
                            <tr>
                                <td><b>${s.id}</b></td>
                                <td>${s.lastName} ${s.firstName}</td>
                                <td>${s.major}</td>
                                <td style="color:var(--muted);">${s.email}</td>
                                <td>
                                    <button class="btn-primary" style="padding:5px 12px; font-size:12.5px;"
                                            onclick="toggleChatDrawer(true); openChatWith('${s.id}', '${(s.lastName + ' ' + s.firstName).replace(/'/g, "\\'")}', 'Оюутан')">
                                        💬 Чат
                                    </button>
                                </td>
                            </tr>
                        `).join('')}
                    </tbody>
                </table>
            </div>
        `;
    }

    wrapper.innerHTML = `<div class="lms-container">${html}</div>`;
}

function switchTeacherTab(tab) {
    closeMenus();
    teacherTab = tab;
    renderTeacherPortal();
}

function teacherAddCourse() {
    const name = document.getElementById('t-course-name').value.trim();
    const credit = parseInt(document.getElementById('t-course-credit').value) || 3;
    const dept = document.getElementById('t-course-dept').value.trim();
    if (!name || !dept) return showToast('Хичээлийн нэр болон тэнхимээ оруулна уу.');

    postToCSharp({ action: 'teacherAddCourse', courseName: name, credit, department: dept });
    document.getElementById('t-course-name').value = '';
}

function teacherAddMaterial() {
    const courseName = document.getElementById('t-mat-course').value;
    const moduleTitle = document.getElementById('t-mat-module').value.trim();
    const itemTitle = document.getElementById('t-mat-title').value.trim();
    const itemType = document.getElementById('t-mat-type').value;
    const fileName = document.getElementById('t-mat-file').value.trim();
    const description = document.getElementById('t-mat-desc').value.trim();

    if (!courseName || !moduleTitle || !itemTitle) return showToast('Хичээл, модуль болон гарчгийг бөглөнө үү.');

    postToCSharp({ action: 'teacherAddMaterial', courseName, moduleTitle, itemTitle, itemType, description, fileName });
    document.getElementById('t-mat-title').value = '';
    document.getElementById('t-mat-desc').value = '';
    document.getElementById('t-mat-file').value = '';
}

function teacherDeleteMaterial(id) {
    if (confirm('Энэ материалыг устгах уу?')) {
        postToCSharp({ action: 'teacherDeleteMaterial', id });
    }
}

function teacherSaveGrade() {
    const studentId = document.getElementById('t-grade-stu').value;
    const courseSel = document.getElementById('t-grade-course');
    const courseName = courseSel.value;
    const credit = parseInt(courseSel.options[courseSel.selectedIndex]?.getAttribute('data-credit')) || 3;
    const score = parseInt(document.getElementById('t-grade-score').value);

    if (!studentId || !courseName || isNaN(score) || score < 0 || score > 100) {
        return showToast('Оноог 0-100 хооронд зөв оруулна уу.');
    }

    postToCSharp({ action: 'teacherSaveGrade', studentId, courseName, credit, score });
    document.getElementById('t-grade-score').value = '';
}

function teacherDeleteGrade(studentId, courseName) {
    if (confirm('Энэ дүнг устгах уу?')) {
        postToCSharp({ action: 'teacherDeleteGrade', studentId, courseName });
    }
}

function teacherAddClassEvent() {
    const courseName = document.getElementById('t-ev-course').value;
    const title = document.getElementById('t-ev-title').value.trim();
    const date = document.getElementById('t-ev-date').value;
    const time = document.getElementById('t-ev-time').value || '09:00';
    if (!title || !date) return showToast('Үйл явдлын нэр болон огноог оруулна уу.');

    postToCSharp({ action: 'teacherAddClassEvent', courseName, title, date, time });
    document.getElementById('t-ev-title').value = '';
}