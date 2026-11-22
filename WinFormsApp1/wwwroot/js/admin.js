let adminTab = 'overview';
let adminStudentSearch = '';
let adminTeacherSearch = '';

function getAllowedTabs(role) {
    if (role === 'editor') return ['news'];
    if (role === 'admissions') return ['admissions', 'courses'];
    if (role === 'finance') return ['finance'];
    // super_admin бүх цэсийг харна
    return ['overview', 'news', 'admissions', 'finance', 'students', 'teachers', 'courses', 'staff'];
}

function renderAdminPortal() {
    const wrapper = document.getElementById('lms-main-wrapper');
    const role = currentUser.staffRole || 'super_admin';
    const allowed = getAllowedTabs(role);

    if (!allowed.includes(adminTab)) {
        adminTab = allowed[0];
    }

    const tabMeta = {
        overview: '📊 Хянах самбар',
        news: '📰 Мэдээ & Зарлал',
        admissions: '🎓 Элсэлт',
        finance: '💰 Санхүү',
        students: '👨‍🎓 Оюутнууд',
        teachers: '👨‍🏫 Багш нар',
        courses: '📚 Хичээл',
        staff: '🛡️ Ажилтнууд'
    };

    const navLinks = document.getElementById('lms-nav-links');
    if (navLinks) {
        navLinks.innerHTML = allowed.map(key =>
            `<div class="lms-link ${adminTab === key ? 'active' : ''}" onclick="switchAdminTab('${key}')">${tabMeta[key]}</div>`
        ).join('');
    }

    let html = '';

    // 1. ХЯНАХ САМБАР (SUPER ADMIN)
    if (adminTab === 'overview') {
        html = `
            <h1 class="page-heading">Систем Удирдлагын Самбар (Super Admin)</h1>
            <div class="greeting">Тавтай морилно уу, ${currentUser.firstName}! 🛡️</div>

            <div style="display:grid; grid-template-columns:repeat(auto-fit, minmax(170px, 1fr)); gap:14px; margin-bottom:24px;">
                <div class="moodle-card" style="margin:0; text-align:center; cursor:pointer;" onclick="switchAdminTab('students')">
                    <div style="font-size:28px; font-weight:800; color:var(--green-main);">${adminData.students.length}</div>
                    <div style="font-size:13px; color:var(--muted);">Оюутан</div>
                </div>
                <div class="moodle-card" style="margin:0; text-align:center; cursor:pointer;" onclick="switchAdminTab('teachers')">
                    <div style="font-size:28px; font-weight:800; color:#0284c7;">${adminData.teachers.length}</div>
                    <div style="font-size:13px; color:var(--muted);">Багш</div>
                </div>
                <div class="moodle-card" style="margin:0; text-align:center; cursor:pointer;" onclick="switchAdminTab('admissions')">
                    <div style="font-size:28px; font-weight:800; color:#7c3aed;">${adminData.admissions.length}</div>
                    <div style="font-size:13px; color:var(--muted);">Элсэгчийн өргөдөл</div>
                </div>
                <div class="moodle-card" style="margin:0; text-align:center; cursor:pointer;" onclick="switchAdminTab('news')">
                    <div style="font-size:28px; font-weight:800; color:#d97706;">${adminData.news.length}</div>
                    <div style="font-size:13px; color:var(--muted);">Нийт мэдээ</div>
                </div>
            </div>

            <div class="moodle-card">
                <div class="card-title">📢 Бүх хэрэглэгчдэд зарлал (Мэдэгдэл) илгээх</div>
                <div class="input-group">
                    <label>Зарлалын утга:</label>
                    <input type="text" id="a-broadcast-msg" placeholder="Жнь: Намрын улирлын шалгалтын хуваарь гарлаа...">
                </div>
                <button class="btn-primary" onclick="adminBroadcastNotif()">📢 Бүх оюутан, багшид илгээх</button>
            </div>
        `;
    }

    // 2. МЭДЭЭ & ЗАРЛАЛ (PR / EDITOR)
    else if (adminTab === 'news') {
        html = `
            <h1 class="page-heading">Мэдээ & Зарлал удирдах (PR / Контент)</h1>

            <div class="moodle-card">
                <div class="card-title">✍️ Шинэ мэдээ, зарлал нийтлэх</div>
                <div class="input-row">
                    <div class="input-group" style="flex:2;">
                        <label>Гарчиг:</label>
                        <input type="text" id="a-news-title" placeholder="Мэдээний гарчиг...">
                    </div>
                    <div class="input-group">
                        <label>Ангилал:</label>
                        <select id="a-news-cat">
                            <option value="Тэргүүн мэдээ">Тэргүүн мэдээ</option>
                            <option value="Мэдээ" selected>Мэдээ</option>
                            <option value="Зарлал">Зарлал</option>
                        </select>
                    </div>
                    <div class="input-group" style="max-width:130px;">
                        <label>Дэвсгэр өнгө:</label>
                        <input type="color" id="a-news-color" value="#0e4d2d" style="height:42px; padding:4px;">
                    </div>
                </div>
                <div class="input-row">
                    <div class="input-group" style="flex:2;">
                        <label>Зураг оруулах (Компьютераас):</label>
                        <input type="file" id="a-news-file" accept="image/*" style="padding:7px;">
                    </div>
                    <div class="input-group">
                        <label>Зургийн байрлал:</label>
                        <select id="a-news-imgpos">
                            <option value="top">Дээд талд (Бүтэн өргөн)</option>
                            <option value="middle">Текстийн дунд</option>
                            <option value="left">Зүүн талд (Текст хажууд нь)</option>
                        </select>
                    </div>
                </div>
                <div class="input-group">
                    <label>Товч тайлбар (Нүүр хуудсанд харагдана):</label>
                    <input type="text" id="a-news-summary" placeholder="1-2 өгүүлбэр товч тайлбар...">
                </div>
                <div class="input-group">
                    <label>Дэлгэрэнгүй агуулга (Мэдээ рүү ороход харагдана):</label>
                    <textarea id="a-news-content" rows="5" style="width:100%; padding:10px; border:1px solid var(--border); border-radius:6px; font-family:inherit;" placeholder="Мэдээний бүрэн эхийг энд бичнэ үү..."></textarea>
                </div>
                <button class="btn-primary" onclick="adminAddNews()">+ Нүүр хуудсанд нийтлэх</button>
            </div>

            <div class="moodle-card">
                <div class="card-title">Нийтэлсэн мэдээ, зарлалууд (${adminData.news.length})</div>
                <table class="grades-table">
                    <thead><tr><th>Ангилал</th><th>Гарчиг</th><th>Огноо</th><th>Үйлдэл</th></tr></thead>
                    <tbody>
                        ${adminData.news.map(n => `
                            <tr>
                                <td><span class="grade-pill">${n.category}</span></td>
                                <td><b>${n.title}</b><br><small style="color:var(--muted);">${n.summary}</small></td>
                                <td style="white-space:nowrap;">${n.date}</td>
                                <td><button class="m-select" style="color:#dc2626; cursor:pointer;" onclick="adminDeleteNews(${n.id})">Устгах</button></td>
                            </tr>
                        `).join('')}
                    </tbody>
                </table>
            </div>
        `;
    }

    // 3. ЭЛСЭЛТИЙН АЛБА (ADMISSIONS)
    else if (adminTab === 'admissions') {
        const isOpen = adminData.admissionOpen;
        html = `
            <h1 class="page-heading">Элсэлтийн удирдлага (Сургалтын алба)</h1>

            <div class="moodle-card" style="display:flex; justify-content:space-between; align-items:center; border-left:5px solid ${isOpen ? '#16a34a' : '#dc2626'};">
                <div>
                    <div style="font-size:16px; font-weight:700;">
                        Элсэлтийн бүртгэлийн төлөв: <span style="color:${isOpen ? '#16a34a' : '#dc2626'};">${isOpen ? '🟢 НЭЭЛТТЭЙ' : '🔴 ХААЛТТАЙ (ДУУССАН)'}</span>
                    </div>
                    <div style="font-size:13px; color:var(--muted); margin-top:4px;">
                        Хаалттай үед нүүр хуудсаас шинээр бүртгүүлэх боломжгүй болж, нэвтрэх цонх руу чиглүүлнэ.
                    </div>
                </div>
                <button class="btn-primary" style="background:${isOpen ? '#dc2626' : '#16a34a'};" onclick="adminToggleAdmission(${isOpen ? 0 : 1})">
                    ${isOpen ? '🔒 Элсэлтийг хаах' : '🔓 Элсэлтийг нээх'}
                </button>
            </div>

            <div class="moodle-card">
                <div class="card-title">Бүртгүүлсэн элсэгчид (${adminData.admissions.length})</div>
                ${adminData.admissions.length === 0 ? `<p style="color:var(--muted); text-align:center; padding:20px;">Одоогоор бүртгүүлсэн элсэгч байхгүй байна.</p>` : `
                <table class="grades-table">
                    <thead><tr><th>Элсэгч</th><th>Утас / Регистр</th><th>Хөтөлбөр</th><th>ЭЕШ / Хөнгөлөлт</th><th>Төлөв</th><th>Шийдвэр</th></tr></thead>
                    <tbody>
                        ${adminData.admissions.map(a => `
                            <tr>
                                <td><b>${a.fullName}</b></td>
                                <td>${a.phone}<br><small style="color:var(--muted);">${a.registerNo}</small></td>
                                <td>${a.programName}</td>
                                <td><b>${a.eyeshScore}</b> (${a.discount}%)<br><small>${a.finalTuition.toLocaleString()}₮</small></td>
                                <td><span class="grade-pill">${a.status}</span></td>
                                <td style="white-space:nowrap;">
                                    ${a.status === 'Бүртгэгдсэн'
                ? `<span style="color:#16a34a; font-weight:700; font-size:12px;">✓ Оюутан болсон</span>`
                : a.status === 'Тэнцсэн'
                    ? `<button class="btn-primary" style="padding:4px 10px; font-size:12px;" onclick="adminConvertApplicant(${a.id})">🎓 Оюутан болгох</button>`
                    : `<button class="m-select" style="color:#16a34a; cursor:pointer; padding:4px 8px;" onclick="adminUpdateAdmission(${a.id}, 'Тэнцсэн')">✓ Тэнцсэн</button>
                                               <button class="m-select" style="color:#dc2626; cursor:pointer; padding:4px 8px;" onclick="adminUpdateAdmission(${a.id}, 'Татгалзсан')">✕</button>`}
                                </td>
                            </tr>
                        `).join('')}
                    </tbody>
                </table>`}
            </div>
        `;
    }

    // 4. САНХҮҮ & ТӨЛБӨР (FINANCE / НЯГТЛАН)
    else if (adminTab === 'finance') {
        html = `
            <h1 class="page-heading">Санхүү & Сургалтын төлбөр (Нягтлан)</h1>

            <div class="moodle-card">
                <div class="card-title">💳 Оюутны төлбөрийн нэхэмжлэх үүсгэх</div>
                <div class="input-row">
                    <div class="input-group">
                        <label>Оюутан:</label>
                        <select id="a-pay-stu">
                            ${adminData.students.map(s => `<option value="${s.id}">${s.id} - ${s.lastName} ${s.firstName}</option>`).join('')}
                        </select>
                    </div>
                    <div class="input-group">
                        <label>Нийт төлбөр (₮):</label>
                        <input type="number" id="a-pay-total" value="3200000">
                    </div>
                    <div class="input-group">
                        <label>Хөнгөлөлт (₮):</label>
                        <input type="number" id="a-pay-disc" value="0">
                    </div>
                    <div class="input-group">
                        <label>Төлсөн дүн (₮):</label>
                        <input type="number" id="a-pay-paid" value="0">
                    </div>
                </div>
                <button class="btn-primary" onclick="adminAddPayment()">+ Төлбөрийн бүртгэл нэмэх</button>
            </div>

            <div class="moodle-card">
                <div class="card-title">Сургалтын төлбөрийн жагсаалт (${adminData.payments.length})</div>
                ${adminData.payments.length === 0 ? `<p style="color:var(--muted); text-align:center; padding:20px;">Төлбөрийн бүртгэл одоогоор алга.</p>` : `
                <table class="grades-table">
                    <thead><tr><th>Оюутны код</th><th>Улирал</th><th>Нийт / Хөнгөлөлт</th><th>Төлсөн</th><th>Төлөв</th><th>Үйлдэл</th></tr></thead>
                    <tbody>
                        ${adminData.payments.map(p => `
                            <tr>
                                <td><b>${p.studentId}</b></td>
                                <td>${p.semester}</td>
                                <td>${p.totalFee.toLocaleString()}₮ <small style="color:#16a34a;">(-${p.discountAmount.toLocaleString()}₮)</small></td>
                                <td><b>${p.paidAmount.toLocaleString()}₮</b></td>
                                <td><span class="grade-pill">${p.status}</span></td>
                                <td style="white-space:nowrap;">
                                    ${p.status !== 'Төлсөн' ? `<button class="btn-primary" style="padding:4px 10px; font-size:12px;" onclick="adminMarkPaid(${p.id})">✓ Төлсөн</button>` : ''}
                                    <button class="m-select" style="cursor:pointer; padding:4px 8px;" onclick="printAdminReceipt(${p.id})">🖨️ Баримт</button>
                                </td>
                            </tr>
                        `).join('')}
                    </tbody>
                </table>`}
            </div>
        `;
    }

    // 5. АЖИЛТНУУД & ЭРХ ОЛГОХ (SUPER ADMIN)
    else if (adminTab === 'staff') {
        const roleLabels = { super_admin: 'Super Admin', editor: 'PR / Контент', admissions: 'Элсэлтийн алба', finance: 'Нягтлан / Санхүү' };
        html = `
            <h1 class="page-heading">Ажилтнууд & Эрхийн удирдлага (RBAC)</h1>

            <div class="moodle-card">
                <div class="card-title">➕ Шинэ ажилтан нэмэх</div>
                <div class="input-row">
                    <div class="input-group"><label>Нэвтрэх нэр:</label><input type="text" id="a-stf-user" placeholder="Жнь: pr2"></div>
                    <div class="input-group"><label>Нууц үг:</label><input type="text" id="a-stf-pass" placeholder="123"></div>
                    <div class="input-group"><label>Овог, Нэр:</label><input type="text" id="a-stf-name" placeholder="Б. Болд"></div>
                </div>
                <div class="input-row">
                    <div class="input-group">
                        <label>Системийн эрх (Role):</label>
                        <select id="a-stf-role">
                            <option value="editor">Контент менежер / PR (Мэдээ, зарлал)</option>
                            <option value="admissions">Сургалт, элсэлтийн алба (Элсэлт)</option>
                            <option value="finance">Нягтлан бодогч (Санхүү, төлбөр)</option>
                            <option value="super_admin">Систем Админ (Бүх эрх)</option>
                        </select>
                    </div>
                    <div class="input-group"><label>Алба / Хэлтэс:</label><input type="text" id="a-stf-dept" placeholder="Албаны нэр..."></div>
                </div>
                <button class="btn-primary" onclick="adminAddStaff()">+ Ажилтан нэмэх</button>
            </div>

            <div class="moodle-card">
                <div class="card-title">Бүртгэлтэй ажилтнууд (${adminData.staff.length})</div>
                <table class="grades-table">
                    <thead><tr><th>Нэвтрэх нэр</th><th>Нэр</th><th>Эрх (Role)</th><th>Алба</th><th>Үйлдэл</th></tr></thead>
                    <tbody>
                        ${adminData.staff.map(st => `
                            <tr>
                                <td><b>${st.username}</b></td>
                                <td>${st.fullName}</td>
                                <td><span class="grade-pill">${roleLabels[st.role] || st.role}</span></td>
                                <td>${st.department}</td>
                                <td>
                                    ${st.username !== 'admin' ? `<button class="m-select" style="color:#dc2626; cursor:pointer;" onclick="adminDeleteStaff('${st.username}')">Устгах</button>` : '<small style="color:var(--muted);">Үндсэн</small>'}
                                </td>
                            </tr>
                        `).join('')}
                    </tbody>
                </table>
            </div>
        `;
    }

    // 6. ОЮУТНУУД
    else if (adminTab === 'students') {
        const filtered = adminData.students.filter(s =>
            s.id.toLowerCase().includes(adminStudentSearch.toLowerCase()) ||
            `${s.lastName} ${s.firstName}`.toLowerCase().includes(adminStudentSearch.toLowerCase())
        );
        html = `
            <h1 class="page-heading">Оюутны бүртгэл удирдах</h1>
            <div class="moodle-card">
                <div class="card-title">➕ Шинэ оюутан бүртгэх</div>
                <div class="input-row">
                    <div class="input-group"><label>Овог:</label><input type="text" id="a-stu-last"></div>
                    <div class="input-group"><label>Нэр:</label><input type="text" id="a-stu-first"></div>
                    <div class="input-group"><label>Оюутны код:</label><input type="text" id="a-stu-id"></div>
                </div>
                <div class="input-row">
                    <div class="input-group"><label>Имэйл:</label><input type="email" id="a-stu-email"></div>
                    <div class="input-group">
                        <label>Мэргэжил:</label>
                        <select id="a-stu-major">
                            <option value="Мэдээллийн технологи">Мэдээллийн технологи</option>
                            <option value="Цахилгаан инженерчлэл">Цахилгаан инженерчлэл</option>
                            <option value="Механик инженерчлэл">Механик инженерчлэл</option>
                            <option value="Барилгын инженерчлэл">Барилгын инженерчлэл</option>
                        </select>
                    </div>
                    <div class="input-group"><label>Нууц үг:</label><input type="text" id="a-stu-pass" value="1234"></div>
                </div>
                <button class="btn-primary" onclick="adminAddStudent()">+ Оюутны хаяг үүсгэх</button>
            </div>
            <div class="moodle-card">
                <div class="card-title">Бүртгэлтэй оюутнууд (${filtered.length})</div>
                <div class="controls-row"><input class="m-input" type="text" placeholder="Хайх..." value="${adminStudentSearch}" oninput="adminStudentSearch=this.value; renderAdminPortal();"></div>
                <table class="grades-table">
                    <thead><tr><th>Код</th><th>Овог, Нэр</th><th>Мэргэжил</th><th>Имэйл</th><th>Үйлдэл</th></tr></thead>
                    <tbody>
                        ${filtered.map(s => `<tr><td><b>${s.id}</b></td><td>${s.lastName} ${s.firstName}</td><td>${s.major}</td><td>${s.email}</td><td><button class="m-select" style="color:#dc2626; cursor:pointer;" onclick="adminDeleteStudent('${s.id}')">Устгах</button></td></tr>`).join('')}
                    </tbody>
                </table>
            </div>
        `;
    }

    // 7. БАГШ НАР
    else if (adminTab === 'teachers') {
        const filtered = adminData.teachers.filter(t => t.name.toLowerCase().includes(adminTeacherSearch.toLowerCase()) || t.id.toLowerCase().includes(adminTeacherSearch.toLowerCase()));
        html = `
            <h1 class="page-heading">Багшийн бүртгэл удирдах</h1>
            <div class="moodle-card">
                <div class="card-title">➕ Шинэ багш бүртгэх</div>
                <div class="input-row">
                    <div class="input-group"><label>Багшийн ID:</label><input type="text" id="a-tch-id"></div>
                    <div class="input-group" style="flex:2;"><label>Овог, Нэр:</label><input type="text" id="a-tch-name"></div>
                </div>
                <div class="input-row">
                    <div class="input-group" style="flex:2;"><label>Тэнхим:</label><input type="text" id="a-tch-dept"></div>
                    <div class="input-group"><label>Нууц үг:</label><input type="text" id="a-tch-pass" value="1234"></div>
                </div>
                <button class="btn-primary" onclick="adminAddTeacher()">+ Багшийн хаяг үүсгэх</button>
            </div>
            <div class="moodle-card">
                <div class="card-title">Бүртгэлтэй багш нар (${filtered.length})</div>
                <table class="grades-table">
                    <thead><tr><th>ID</th><th>Овог, Нэр</th><th>Тэнхим</th><th>Үйлдэл</th></tr></thead>
                    <tbody>
                        ${filtered.map(t => `<tr><td><b>${t.id}</b></td><td>${t.name}</td><td>${t.department}</td><td><button class="m-select" style="color:#dc2626; cursor:pointer;" onclick="adminDeleteTeacher('${t.id}')">Устгах</button></td></tr>`).join('')}
                    </tbody>
                </table>
            </div>
        `;
    }

    // 8. ХИЧЭЭЛҮҮД
    else if (adminTab === 'courses') {
        html = `
            <h1 class="page-heading">Хичээлийн сан удирдах</h1>
            <div class="moodle-card">
                <div class="card-title">➕ Шинэ хичээл нэмэх</div>
                <div class="input-row">
                    <div class="input-group" style="flex:2;"><label>Хичээлийн нэр:</label><input type="text" id="a-course-name"></div>
                    <div class="input-group"><label>Кредит:</label><input type="number" id="a-course-credit" value="3"></div>
                    <div class="input-group"><label>Тэнхим:</label><input type="text" id="a-course-dept"></div>
                </div>
                <button class="btn-primary" onclick="adminAddCourse()">+ Хичээл нэмэх</button>
            </div>
            <div class="moodle-card">
                <div class="card-title">Нийт хичээлүүд (${adminData.courses.length})</div>
                <table class="grades-table">
                    <thead><tr><th>Хичээлийн нэр</th><th>Кредит</th><th>Тэнхим</th><th>Үйлдэл</th></tr></thead>
                    <tbody>
                        ${adminData.courses.map(c => `<tr><td style="color:var(--green-main); font-weight:600;">${c.name}</td><td>${c.credit}</td><td>${c.department}</td><td><button class="m-select" style="color:#dc2626; cursor:pointer;" onclick="adminDeleteCourse('${c.name.replace(/'/g, "\\'")}')">Устгах</button></td></tr>`).join('')}
                    </tbody>
                </table>
            </div>
        `;
    }

    wrapper.innerHTML = `<div class="lms-container">${html}</div>`;
}

function switchAdminTab(tab) { closeMenus(); adminTab = tab; renderAdminPortal(); }

// === ACTIONS ===
function adminAddNews() {
    const title = document.getElementById('a-news-title').value.trim();
    const category = document.getElementById('a-news-cat').value;
    const imageColor = document.getElementById('a-news-color').value;
    const imagePosition = document.getElementById('a-news-imgpos').value;
    const summary = document.getElementById('a-news-summary').value.trim();
    const content = document.getElementById('a-news-content').value.trim() || summary;
    const fileInput = document.getElementById('a-news-file');

    if (!title) return showToast('Мэдээний гарчиг оруулна уу.');

    if (fileInput && fileInput.files && fileInput.files[0]) {
        const file = fileInput.files[0];
        const reader = new FileReader();
        reader.onload = function (e) {
            postToCSharp({
                action: 'adminAddNews',
                title, category, summary, content, imageColor, imagePosition,
                imageBase64: e.target.result,
                imageFileName: file.name
            });
        };
        reader.readAsDataURL(file);
    } else {
        postToCSharp({
            action: 'adminAddNews',
            title, category, summary, content, imageColor, imagePosition,
            imageBase64: '',
            imageFileName: ''
        });
    }

    document.getElementById('a-news-title').value = '';
    document.getElementById('a-news-summary').value = '';
    document.getElementById('a-news-content').value = '';
    if (fileInput) fileInput.value = '';
}

// Админаас оюутан/багшийн зураг солих функц
function adminTriggerAvatarUpload(targetRole, targetId) {
    const inp = document.createElement('input');
    inp.type = 'file';
    inp.accept = 'image/*';
    inp.onchange = function () {
        if (!inp.files || !inp.files[0]) return;
        const file = inp.files[0];
        const reader = new FileReader();
        reader.onload = function (e) {
            postToCSharp({
                action: 'uploadAvatar',
                targetRole,
                targetId,
                imageBase64: e.target.result,
                imageFileName: file.name
            });
        };
        reader.readAsDataURL(file);
    };
    inp.click();
}
function adminDeleteNews(id) { if (confirm('Энэ мэдээг устгах уу?')) postToCSharp({ action: 'adminDeleteNews', id }); }

function adminToggleAdmission(openVal) { postToCSharp({ action: 'adminToggleAdmission', isOpen: openVal === 1 }); }
function adminUpdateAdmission(id, status) { postToCSharp({ action: 'adminUpdateAdmissionStatus', id, status }); }

function adminAddPayment() {
    const studentId = document.getElementById('a-pay-stu').value;
    const totalFee = parseInt(document.getElementById('a-pay-total').value) || 0;
    const discountAmount = parseInt(document.getElementById('a-pay-disc').value) || 0;
    const paidAmount = parseInt(document.getElementById('a-pay-paid').value) || 0;
    postToCSharp({ action: 'adminAddPayment', studentId, totalFee, discountAmount, paidAmount });
}
function adminMarkPaid(id) { postToCSharp({ action: 'adminMarkPaymentPaid', id }); }

function adminAddStaff() {
    const username = document.getElementById('a-stf-user').value.trim();
    const password = document.getElementById('a-stf-pass').value.trim();
    const fullName = document.getElementById('a-stf-name').value.trim();
    const role = document.getElementById('a-stf-role').value;
    const department = document.getElementById('a-stf-dept').value.trim();
    if (!username || !password || !fullName) return showToast('Бүх талбарыг бөглөнө үү.');
    postToCSharp({ action: 'adminAddStaff', username, password, fullName, role, department });
}
function adminDeleteStaff(username) { if (confirm(`"${username}" ажилтныг устгах уу?`)) postToCSharp({ action: 'adminDeleteStaff', username }); }

function adminAddStudent() {
    const lastName = document.getElementById('a-stu-last').value.trim();
    const firstName = document.getElementById('a-stu-first').value.trim();
    const studentId = document.getElementById('a-stu-id').value.trim();
    const email = document.getElementById('a-stu-email').value.trim();
    const major = document.getElementById('a-stu-major').value;
    const password = document.getElementById('a-stu-pass').value.trim();
    if (!lastName || !firstName || !studentId || !email || !password) return showToast('Бүх талбарыг бөглөнө үү.');
    postToCSharp({ action: 'adminAddStudent', lastName, firstName, studentId, email, major, password });
}
function adminDeleteStudent(studentId) { if (confirm(`"${studentId}" оюутныг устгах уу?`)) postToCSharp({ action: 'adminDeleteStudent', studentId }); }

function adminAddTeacher() {
    const teacherId = document.getElementById('a-tch-id').value.trim();
    const fullName = document.getElementById('a-tch-name').value.trim();
    const department = document.getElementById('a-tch-dept').value.trim();
    const password = document.getElementById('a-tch-pass').value.trim();
    if (!teacherId || !fullName || !department || !password) return showToast('Бүх талбарыг бөглөнө үү.');
    postToCSharp({ action: 'adminAddTeacher', teacherId, fullName, department, password });
}
function adminDeleteTeacher(teacherId) { if (confirm(`"${teacherId}" багшийг устгах уу?`)) postToCSharp({ action: 'adminDeleteTeacher', teacherId }); }

function adminAddCourse() {
    const courseName = document.getElementById('a-course-name').value.trim();
    const credit = parseInt(document.getElementById('a-course-credit').value) || 3;
    const department = document.getElementById('a-course-dept').value.trim();
    if (!courseName || !department) return showToast('Бүх талбарыг бөглөнө үү.');
    postToCSharp({ action: 'adminAddCourse', courseName, credit, department });
}
function adminDeleteCourse(courseName) { if (confirm(`"${courseName}" хичээлийг устгах уу?`)) postToCSharp({ action: 'adminDeleteCourse', courseName }); }

function adminBroadcastNotif() {
    const message = document.getElementById('a-broadcast-msg').value.trim();
    if (!message) return showToast('Зарлалын утгаа бичнэ үү.');
    postToCSharp({ action: 'adminBroadcastNotif', message });
    document.getElementById('a-broadcast-msg').value = '';
}

function adminConvertApplicant(id) {
    if (confirm('Энэ элсэгчид оюутны код, нууц үг (1234) болон төлбөрийн нэхэмжлэх автоматаар үүсгэх үү?')) {
        postToCSharp({ action: 'adminConvertApplicant', id });
    }
}

function printAdminReceipt(payId) {
    const p = adminData.payments.find(x => x.id === payId);
    if (!p) return;
    const net = Math.max(0, p.totalFee - p.discountAmount);
    printDocument('Сургалтын төлбөрийн баримт (Санхүү)', `
        <p><b>Оюутны код:</b> ${p.studentId}   |   <b>Улирал:</b> ${p.semester}</p>
        <table class="grades-table" style="margin-top:14px;">
            <tr><td>Үндсэн төлбөр</td><td><b>${p.totalFee.toLocaleString()}₮</b></td></tr>
            <tr><td>Хөнгөлөлт</td><td>-${p.discountAmount.toLocaleString()}₮</td></tr>
            <tr><td>Нийт төлөх</td><td><b>${net.toLocaleString()}₮</b></td></tr>
            <tr><td>Төлсөн дүн</td><td><b>${p.paidAmount.toLocaleString()}₮</b></td></tr>
            <tr><td>Төлөв</td><td><b>${p.status}</b></td></tr>
        </table>
    `);
}