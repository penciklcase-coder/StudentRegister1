// ================= ДУНДЫН ТӨЛӨВ (GLOBAL STATE) =================
let currentRole = 'student';
let currentUser = null;
let currentLmsTab = 'dashboard';
let activeCourseName = null;
let highlightActivityTitle = null;
let editMode = false;

// MySQL-ээс ирэх өгөгдлүүд
let studentData = { available: [], enrolled: [], grades: [], payments: [] };
let teacherData = { courses: [], students: [], grades: [], materials: [] };
let adminData = { students: [], teachers: [], courses: [], totalGrades: 0, staff: [], news: [], admissions: [], payments: [], admissionOpen: true };
let courseMaterials = [];
let calendarEvents = [];
let notificationsData = [];
let chatContacts = [];
let chatMessages = [];
let activeChatPartner = null;

function postToCSharp(payload) {
    if (window.chrome && window.chrome.webview) {
        window.chrome.webview.postMessage(payload);
    }
}

function showView(id) {
    document.querySelectorAll('.view').forEach(v => v.classList.remove('active'));
    document.getElementById(id).classList.add('active');
    const err = document.getElementById('login-error');
    if (err) err.style.display = 'none';
}

function setRole(role, btn) {
    currentRole = role;
    document.querySelectorAll('.role-btn').forEach(b => b.classList.remove('active'));
    btn.classList.add('active');
    document.getElementById('lbl-id').innerText = t('id_' + role);
    document.getElementById('login-error').style.display = 'none';
}

function sendLogin() {
    const id = document.getElementById('txt-id').value.trim();
    const password = document.getElementById('txt-pass').value.trim();
    if (!id || !password) {
        showLoginError(currentLang === 'mn' ? 'Код болон нууц үгээ бүрэн оруулна уу.' : 'Please enter ID and password.');
        return;
    }
    postToCSharp({ action: 'login', role: currentRole, id, password });
}

function showLoginError(msg) {
    const el = document.getElementById('login-error');
    el.innerText = msg;
    el.style.display = 'block';
}

function logout() {
    closeMenus();
    toggleChatDrawer(false);
    isGroupChatOpen = false;
    currentUser = null;
    activeCourseName = null;
    document.getElementById('txt-pass').value = '';

    // Оюутны үндсэн навигацийн цэсийг буцааж сэргээх
    const navLinks = document.getElementById('lms-nav-links');
    if (navLinks) {
        navLinks.innerHTML = `
            <div class="lms-link" id="nav-home" onclick="switchLmsTab('home')" data-i18n="nav_home">🏠 Нүүр</div>
            <div class="lms-link active" id="nav-dashboard" onclick="switchLmsTab('dashboard')" data-i18n="nav_dashboard">📊 Хянах самбар</div>
            <div class="lms-link" id="nav-courses" onclick="switchLmsTab('courses')" data-i18n="nav_courses">📚 Миний хичээлүүд</div>
        `;
        applyTranslations();
    }
    showView('view-login');
}

function switchLmsTab(tab) {
    closeMenus();
    isGroupChatOpen = false;
    currentLmsTab = tab;
    if (tab !== 'courseDetail') {
        activeCourseName = null;
        highlightActivityTitle = null;
    }

    document.querySelectorAll('.lms-link').forEach(l => l.classList.remove('active'));
    const navEl = document.getElementById('nav-' + tab);
    if (navEl) navEl.classList.add('active');

    renderCurrentLmsView();
}

function renderCurrentLmsView() {
    if (!currentUser) return;

    if (currentUser.role === 'teacher' && typeof renderTeacherPortal === 'function') {
        renderTeacherPortal();
        return;
    }
    if (currentUser.role === 'admin' && typeof renderAdminPortal === 'function') {
        renderAdminPortal();
        return;
    }

    if (currentLmsTab === 'courseDetail' && activeCourseName) {
        renderCourseDetailPage(activeCourseName, highlightActivityTitle);
    } else {
        renderStudentPage(currentLmsTab);
    }
}

function toggleDropdown() {
    document.getElementById('notif-popup').classList.remove('open');
    renderUserDropdown();
    document.getElementById('user-dropdown').classList.toggle('open');
}

function toggleNotif() {
    document.getElementById('user-dropdown').classList.remove('open');
    document.getElementById('notif-popup').classList.toggle('open');
    renderNotifications();
}

function closeMenus() {
    document.getElementById('user-dropdown').classList.remove('open');
    document.getElementById('notif-popup').classList.remove('open');
}

function toggleEditMode() {
    editMode = !editMode;
    document.getElementById('edit-switch').classList.toggle('on', editMode);
    showToast(editMode ? 'Edit mode ON' : 'Edit mode OFF');
}

function showToast(msg) {
    const el = document.getElementById('toast');
    el.innerText = msg;
    el.classList.add('show');
    clearTimeout(window._toastTimer);
    window._toastTimer = setTimeout(() => el.classList.remove('show'), 3000);
}

function getInitials(last, first) {
    return ((last ? last[0] : '') + (first ? first[0] : '')).toUpperCase() || 'ОЮ';
}

// ================= C# -> JS МЕССЕЖ ХҮЛЭЭН АВАГЧ =================
if (window.chrome && window.chrome.webview) {
    window.chrome.webview.addEventListener('message', event => {
        const res = event.data;

        switch (res.action) {
            case 'loginResult':
                if (res.success) {
                    currentUser = res;
                    document.getElementById('nav-username').innerText = res.firstName;
                    document.getElementById('nav-initials').innerText = getInitials(res.lastName, res.firstName);
                    showView('view-lms');
                    applyTranslations();
                    updateNavAvatar();
                    renderUserDropdown();

                    if (res.role === 'teacher') {
                        teacherTab = 'overview';
                        postToCSharp({ action: 'getTeacherData' });
                    } else if (res.role === 'admin') {
                        adminTab = 'overview';
                        postToCSharp({ action: 'getAdminData' });
                    } else {
                        switchLmsTab('dashboard');
                        postToCSharp({ action: 'getStudentData', studentId: res.id });
                        postToCSharp({ action: 'getCalendarEvents', userId: res.id });
                    }

                    postToCSharp({ action: 'getNotifications', userId: res.id });
                    postToCSharp({ action: 'getContacts', userId: res.id });
                } else {
                    showLoginError(res.message);
                }
                break;

            case 'studentData':
                studentData.available = res.available || [];
                studentData.enrolled = res.enrolled || [];
                studentData.grades = res.grades || [];
                studentData.payments = res.payments || [];
                renderCurrentLmsView();
                break;

            case 'teacherData':
                teacherData.courses = res.courses || [];
                teacherData.students = res.students || [];
                teacherData.grades = res.grades || [];
                teacherData.materials = res.materials || [];
                renderCurrentLmsView();
                break;

            case 'adminData':
                adminData.students = res.students || [];
                adminData.teachers = res.teachers || [];
                adminData.courses = res.courses || [];
                adminData.totalGrades = res.totalGrades || 0;
                adminData.staff = res.staff || [];
                adminData.news = res.news || [];
                adminData.admissions = res.admissions || [];
                adminData.payments = res.payments || [];
                adminData.admissionOpen = res.admissionOpen;
                renderCurrentLmsView();
                break;

            case 'courseMaterials':
                courseMaterials = res.materials || [];
                if (currentLmsTab === 'courseDetail') {
                    renderCourseDetailPage(activeCourseName, highlightActivityTitle);
                }
                break;

            case 'calendarEvents':
                calendarEvents = res.events || [];
                if (currentLmsTab === 'dashboard' || currentLmsTab === 'calendar') {
                    renderCurrentLmsView();
                }
                break;

            case 'notifications':
                notificationsData = res.items || [];
                updateNotifBadge();
                renderNotifications();
                break;

            case 'contacts':
                chatContacts = res.contacts || [];
                renderChatDrawer();
                break;

            case 'messages':
                chatMessages = res.messages || [];
                renderChatDrawer();
                break;

            case 'publicData':
                publicData.admissionOpen = res.admissionOpen;
                publicData.programs = res.programs || [];
                publicData.news = res.news || [];
                renderPublicHome();
                if (document.getElementById('view-admissions')?.classList.contains('active')) {
                    renderAdmissionsPage();
                }
                break;

            case 'admissionSuccess':
                showToast(res.message);
                showView('view-home');
                postToCSharp({ action: 'getPublicData' });
                break;

            case 'avatarUpdated':
                if (currentUser && currentUser.id === res.targetId) {
                    currentUser.avatarUrl = res.avatarUrl;
                    updateNavAvatar();
                }
                showToast('Профайл зураг амжилттай шинэчлэгдлээ!');
                break;

            case 'privateFilesData':
                privateFilesList = res.files || [];
                renderPrivateFilesPage();
                break;

            case 'courseParticipantsData':
                if (typeof onCourseParticipantsReceived === 'function') {
                    onCourseParticipantsReceived(res.participants || [], res.courseGrades || []);
                }
                break;

            case 'groupChatList':
                chatGroupsList = res.groups || [];
                chatAllUsersList = res.users || [];
                if (isGroupChatOpen) renderGroupChatPage();
                break;

            case 'groupMessagesData':
                if (activeChatGroupId === res.groupId) {
                    activeGroupMessages = res.messages || [];
                    if (isGroupChatOpen) renderGroupChatPage();
                }
                break;

            case 'courseAttendanceData':
                attCourseName = res.courseName || '';
                attSelectedDate = res.date || new Date().toISOString().slice(0, 10);
                teacherAttList = res.list || [];
                if (isTeacherAttOpen) renderTeacherAttendancePage();
                break;

            case 'studentAttendanceData':
                studentAttRecords = res.records || [];
                if (isStudentAttOpen) renderStudentAttendancePage();
                break;

            case 'submissionsData':
                submissionsList = res.list || [];
                if (isAssignmentsOpen) renderAssignmentsPage();
                if (currentLmsTab === 'courseDetail' && activeCourseName) {
                    renderCourseDetailPage(activeCourseName, highlightActivityTitle);
                }
                break;

            case 'timetableData':
                timetableSlots = res.slots || [];
                if (isTimetableOpen) renderTimetablePage();
                break;

            case 'toast':
                showToast(res.message);
                break;
        }
    });
}

document.addEventListener('DOMContentLoaded', () => {
    initSavedUiPrefs();
    applyTranslations();
    postToCSharp({ action: 'getPublicData' });
});

// ================= SIDEBAR, 4 THEME PRESETS & PROFILE SETTINGS =================
function updateNavAvatar() {
    const el = document.getElementById('nav-initials');
    if (!el || !currentUser) return;
    if (currentUser.avatarUrl) {
        el.innerHTML = `<img src="${currentUser.avatarUrl}" style="width:100%; height:100%; object-fit:cover; border-radius:50%;">`;
        el.style.padding = '0';
    } else {
        el.innerText = getInitials(currentUser.lastName, currentUser.firstName);
    }
}

function toggleSidebar() {
    const sb = document.getElementById('lms-sidebar');
    if (!sb) return;
    const isCollapsed = sb.classList.toggle('collapsed');
    localStorage.setItem('kosen_sidebar_collapsed', isCollapsed ? '1' : '0');
}

function applyTheme(themeKey) {
    document.body.classList.remove('dark-mode');
    if (themeKey === 'emerald') {
        document.body.removeAttribute('data-theme');
    } else {
        document.body.setAttribute('data-theme', themeKey);
    }
    localStorage.setItem('kosen_theme', themeKey);
}

function initSavedUiPrefs() {
    const savedTheme = localStorage.getItem('kosen_theme') || 'emerald';
    applyTheme(savedTheme);
    if (localStorage.getItem('kosen_sidebar_collapsed') === '1') {
        document.getElementById('lms-sidebar')?.classList.add('collapsed');
    }
}

function openSettingsModal() {
    closeMenus();
    if (!currentUser) return;

    let modal = document.getElementById('universal-settings-modal');
    if (!modal) {
        modal = document.createElement('div');
        modal.id = 'universal-settings-modal';
        modal.className = 'modal-overlay';
        modal.onclick = (e) => { if (e.target === modal) modal.classList.remove('open'); };
        document.body.appendChild(modal);
    }

    const currentTheme = localStorage.getItem('kosen_theme') || 'emerald';
    const themes = [
        { key: 'emerald', name: 'KOSEN Emerald', desc: 'Албан ёсны ногоон', color: 'linear-gradient(135deg, #0b3b24, #16a34a)' },
        { key: 'warm', name: 'Warm Sand', desc: 'Дулаан, нүд амраах', color: 'linear-gradient(135deg, #78350f, #f6f1e7)' },
        { key: 'slate', name: 'Midnight Slate', desc: 'Гүн хөх бараан', color: 'linear-gradient(135deg, #0b1120, #38bdf8)' },
        { key: 'obsidian', name: 'Obsidian Mono', desc: 'Хар + Алтлаг', color: 'linear-gradient(135deg, #09090b, #f59e0b)' }
    ];

    const avatarHtml = currentUser.avatarUrl
        ? `<img src="${currentUser.avatarUrl}" style="width:72px; height:72px; border-radius:50%; object-fit:cover; border:3px solid var(--accent);">`
        : `<div class="avatar-circle" style="width:72px; height:72px; font-size:22px;">${getInitials(currentUser.lastName, currentUser.firstName)}</div>`;

    modal.innerHTML = `
        <div class="modal-box" style="max-width:500px; width:92%;">
            <div style="display:flex; justify-content:space-between; align-items:center; border-bottom:1px solid var(--border); padding-bottom:12px; margin-bottom:16px;">
                <h3 style="margin:0;">⚙️ Профайл ба Өнгөний тохиргоо</h3>
                <button onclick="document.getElementById('universal-settings-modal').classList.remove('open')" style="background:none; border:none; font-size:18px; cursor:pointer; color:var(--text-main);">✕</button>
            </div>

            <div style="display:flex; align-items:center; gap:16px; margin-bottom:18px;">
                ${avatarHtml}
                <div>
                    <div style="font-weight:700; font-size:16px;">${currentUser.lastName || ''} ${currentUser.firstName}</div>
                    <div style="font-size:12.5px; color:var(--muted); margin-bottom:8px;">ID: ${currentUser.id} • ${currentUser.major || ''}</div>
                    <label class="btn-primary" style="padding:6px 12px; font-size:12px; cursor:pointer; display:inline-block;">
                        📷 Профайл зураг солих
                        <input type="file" accept="image/*" style="display:none;" onchange="uploadMyAvatar(this)">
                    </label>
                </div>
            </div>

            <div style="font-weight:700; font-size:13.5px; margin-bottom:8px;">🎨 Системийн өнгөний загвар</div>
            <div class="theme-grid">
                ${themes.map(th => `
                    <div class="theme-card ${currentTheme === th.key ? 'active' : ''}"
                         onclick="applyTheme('${th.key}'); openSettingsModal();">
                        <div class="theme-dot" style="background:${th.color};"></div>
                        <div>
                            <div style="font-weight:700; font-size:13px;">${th.name}</div>
                            <div style="font-size:11px; color:var(--muted);">${th.desc}</div>
                        </div>
                    </div>
                `).join('')}
            </div>

            <div style="border-top:1px solid var(--border); padding-top:14px;">
                <div style="font-weight:700; font-size:13.5px; margin-bottom:10px;">🔑 Нууц үг солих</div>
                <div class="input-group">
                    <input type="password" id="set-old-pass" placeholder="Одоогийн нууц үг">
                </div>
                <div class="input-group">
                    <input type="password" id="set-new-pass" placeholder="Шинэ нууц үг">
                </div>
                <button class="btn-primary" style="width:100%;" onclick="submitPasswordChange()">Нууц үг шинэчлэх</button>
            </div>
        </div>
    `;
    modal.classList.add('open');
}

function uploadMyAvatar(input) {
    if (!input.files || !input.files[0] || !currentUser) return;
    const file = input.files[0];
    const reader = new FileReader();
    reader.onload = function (e) {
        postToCSharp({
            action: 'uploadAvatar',
            targetRole: currentUser.role,
            targetId: currentUser.id,
            imageBase64: e.target.result,
            imageFileName: file.name
        });
        document.getElementById('universal-settings-modal').classList.remove('open');
    };
    reader.readAsDataURL(file);
}

function submitPasswordChange() {
    const oldPass = document.getElementById('set-old-pass').value.trim();
    const newPass = document.getElementById('set-new-pass').value.trim();
    if (!oldPass || !newPass) return showToast('Одоогийн болон шинэ нууц үгээ оруулна уу.');
    postToCSharp({
        action: 'changePassword',
        role: currentUser.role,
        userId: currentUser.id,
        oldPass,
        newPass
    });
    document.getElementById('universal-settings-modal').classList.remove('open');
}

// ================= ЭРХЭЭС ХАМААРСАН ХЭРЭГЛЭГЧИЙН ЦЭС =================
function renderUserDropdown() {
    const dd = document.getElementById('user-dropdown');
    if (!dd || !currentUser) return;

    const role = currentUser.role;
    let itemsHtml = '';

    if (role === 'student') {
        itemsHtml = `
            <div class="dd-item" onclick="openUniversalProfile()">👤 Профайл</div>
            <div class="dd-item" onclick="switchLmsTab('grades')">📊 Дүнгийн жагсаалт</div>
            <div class="dd-item" onclick="switchLmsTab('calendar')">📅 Календарь</div>
            <div class="dd-item" onclick="openTimetablePage()">🗓️ Хичээлийн хуваарь</div>
            <div class="dd-item" onclick="switchLmsTab('tuition')">💳 Сургалтын төлбөр</div>
            <div class="dd-item" onclick="openStudentAttendancePage()">📋 Миний ирц</div>
            <div class="dd-item" onclick="openAssignmentsPage()">📤 Даалгавар илгээх</div>
            <div class="dd-item" onclick="openPrivateFiles()">📁 Хувийн файлууд</div>
            <div class="dd-divider"></div>
            <div class="dd-item" onclick="openSettingsModal()">⚙️ Тохиргоо</div>
            <div class="dd-item" onclick="toggleLanguage()">🌐 Хэл (${currentLang.toUpperCase()})</div>
            <div class="dd-divider"></div>
            <div class="dd-item" style="color:#dc2626;" onclick="logout()">🚪 Гарах</div>
        `;
    } else if (role === 'teacher') {
        itemsHtml = `
            <div class="dd-item" onclick="openUniversalProfile()">👤 Миний профайл</div>
            <div class="dd-item" onclick="switchTeacherTab('courses')">📚 Миний хичээлүүд</div>
            <div class="dd-item" onclick="openTimetablePage()">🗓️ Хичээлийн хуваарь</div>
            <div class="dd-item" onclick="openTeacherAttendancePage()">📋 Ирц бүртгэх</div>
            <div class="dd-item" onclick="openAssignmentsPage()">📝 Ирсэн даалгаврууд</div>
            <div class="dd-item" onclick="openPrivateFiles()">📁 Хувийн файлууд</div>
            <div class="dd-divider"></div>
            <div class="dd-item" onclick="openSettingsModal()">⚙️ Тохиргоо</div>
            <div class="dd-item" onclick="toggleLanguage()">🌐 Хэл (${currentLang.toUpperCase()})</div>
            <div class="dd-divider"></div>
            <div class="dd-item" style="color:#dc2626;" onclick="logout()">🚪 Гарах</div>
        `;
    } else {
        // Admin & Staff (Super Admin, PR, Admissions, Finance)
        itemsHtml = `
            <div class="dd-item" onclick="openUniversalProfile()">👤 Миний профайл</div>
            <div class="dd-item" onclick="openTimetablePage()">🗓️ Хичээлийн хуваарь</div>
            <div class="dd-item" onclick="openPrivateFiles()">📁 Хувийн файлууд</div>
            <div class="dd-item" onclick="openSettingsModal()">⚙️ Тохиргоо</div>
            <div class="dd-item" onclick="toggleLanguage()">🌐 Хэл (${currentLang.toUpperCase()})</div>
            <div class="dd-divider"></div>
            <div class="dd-item" style="color:#dc2626;" onclick="logout()">🚪 Гарах</div>
        `;
    }

    dd.innerHTML = itemsHtml;

    // Оюутан биш бол "Горимыг засварлах" (Edit mode) шилжүүлэгчийг нуух
    const editWrap = document.getElementById('edit-switch')?.parentElement;
    if (editWrap) {
        editWrap.style.display = (role === 'student') ? 'flex' : 'none';
    }
}

// ================= БҮХ ЭРХЭД АЖИЛЛАХ ПРОФАЙЛ + ЦАХИМ ҮНЭМЛЭХ =================
let idCardShowBack = false;

function openUniversalProfile() {
    closeMenus();
    isGroupChatOpen = false;
    isTimetableOpen = false;
    isAssignmentsOpen = false;
    isTeacherAttOpen = false;
    isStudentAttOpen = false;
    idCardShowBack = false;
    if (!currentUser) return;

    document.querySelectorAll('.lms-link').forEach(l => l.classList.remove('active'));
    renderUniversalProfileView();
}

function toggleIdCardSide() {
    idCardShowBack = !idCardShowBack;
    renderUniversalProfileView();
}

function generateOfflineQrSvg(text) {
    const size = 15;
    let hash = 2166136261;
    for (let i = 0; i < text.length; i++) {
        hash ^= text.charCodeAt(i);
        hash = Math.imul(hash, 16777619);
    }

    const isFinder = (r, c) =>
        (r < 5 && c < 5) || (r < 5 && c >= size - 5) || (r >= size - 5 && c < 5);

    const isFinderFilled = (r, c) => {
        const lr = r >= size - 5 ? r - (size - 5) : r;
        const lc = c >= size - 5 ? c - (size - 5) : c;
        return lr === 0 || lr === 4 || lc === 0 || lc === 4 || (lr === 2 && lc === 2);
    };

    let rects = '';
    for (let r = 0; r < size; r++) {
        for (let c = 0; c < size; c++) {
            let filled = false;
            if (isFinder(r, c)) {
                filled = isFinderFilled(r, c);
            } else {
                const seed = Math.imul(hash ^ (r * 31 + c * 17), 2654435761) >>> 0;
                filled = (seed % 100) < 48;
            }
            if (filled) {
                rects += `<rect x="${c * 6 + 6}" y="${r * 6 + 6}" width="6" height="6" fill="#0f172a"/>`;
            }
        }
    }
    const dim = size * 6 + 12;
    return `<svg viewBox="0 0 ${dim} ${dim}" width="96" height="96" style="background:#ffffff; border-radius:8px; border:1px solid #e2e8f0; display:block;">${rects}</svg>`;
}

function renderUniversalProfileView() {
    const wrapper = document.getElementById('lms-main-wrapper');
    if (!wrapper || !currentUser) return;

    const roleNames = {
        student: 'Оюутан',
        teacher: 'Багш',
        super_admin: 'Систем Админ (Super Admin)',
        editor: 'Контент менежер / PR',
        admissions: 'Сургалт, элсэлтийн алба',
        finance: 'Санхүү, нягтлан бодогч'
    };

    const roleLabel = currentUser.role === 'admin'
        ? (roleNames[currentUser.staffRole] || 'Ажилтан')
        : (roleNames[currentUser.role] || currentUser.role);

    const cardTitle = currentUser.role === 'student'
        ? 'ОЮУТНЫ ЦАХИМ ҮНЭМЛЭХ'
        : currentUser.role === 'teacher'
            ? 'БАГШИЙН ЦАХИМ ҮНЭМЛЭХ'
            : 'АЖИЛТНЫ ЦАХИМ ҮНЭМЛЭХ';

    const fullName = `${currentUser.lastName || ''} ${currentUser.firstName}`.trim();

    const avatarHtml = currentUser.avatarUrl
        ? `<img src="${currentUser.avatarUrl}" style="width:96px; height:96px; border-radius:50%; object-fit:cover; border:3px solid var(--accent);">`
        : `<div class="avatar-circle" style="width:96px; height:96px; font-size:30px;">${getInitials(currentUser.lastName, currentUser.firstName)}</div>`;

    const cardPhotoHtml = currentUser.avatarUrl
        ? `<img src="${currentUser.avatarUrl}" class="idcard-photo">`
        : `<div class="idcard-photo idcard-initials">${getInitials(currentUser.lastName, currentUser.firstName)}</div>`;

    const qrSvg = generateOfflineQrSvg(`KOSEN|${currentUser.id}|${fullName}`);

    const cardFrontHtml = `
        <div class="idcard idcard-front">
            <div class="idcard-top">
                <div class="idcard-logo">ХААИС<br>КООСЭН</div>
                <div style="flex:1;">
                    <div class="idcard-school">ДАРХАН КООСЭН ТЕХНОЛОГИЙН КОЛЛЕЖ</div>
                    <div class="idcard-type">${cardTitle}</div>
                </div>
                <span class="idcard-valid">✓ ХҮЧИНТЭЙ</span>
            </div>

            <div class="idcard-body">
                ${cardPhotoHtml}
                <div class="idcard-info">
                    <div class="idcard-name">${fullName}</div>
                    <div class="idcard-role">${roleLabel}</div>
                    <div class="idcard-meta-row"><span>Код / ID:</span> <b>${currentUser.id}</b></div>
                    <div class="idcard-meta-row"><span>Салбар:</span> <b>${currentUser.major || 'Дархан Коосэн'}</b></div>
                </div>
            </div>

            <div class="idcard-bottom">
                <span>Хичээлийн жил: 2026–2027</span>
                <span>DIGITAL ID • MONGOLIA</span>
            </div>
        </div>
    `;

    const cardBackHtml = `
        <div class="idcard idcard-back">
            <div class="idcard-magstrip"></div>
            <div class="idcard-back-body">
                <div>
                    ${qrSvg}
                    <div style="font-size:10px; text-align:center; margin-top:4px; opacity:0.75; font-weight:700;">${currentUser.id}</div>
                </div>
                <div class="idcard-back-info">
                    <div style="font-weight:800; font-size:13px; margin-bottom:6px;">ХААИС-ийн харьяа Дархан Коосэн</div>
                    <div>📍 Дархан-Уул аймаг, Дархан сум</div>
                    <div>📞 Утас: 7037-3737</div>
                    <div>✉️ Имэйл: ${currentUser.email || 'info@kosen.edu.mn'}</div>
                    <div class="idcard-note">
                        Энэхүү үнэмлэх нь сургуулийн өмч бөгөөд номын сан, лаборатори болон шалгалтад нэвтрэхэд ашиглана. Олсон тохиолдолд сургалтын албанд хүлээлгэн өгнө үү.
                    </div>
                </div>
            </div>
        </div>
    `;

    wrapper.innerHTML = `
        <div class="lms-container" style="max-width:860px;">
            <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:18px;">
                <h1 class="page-heading" style="margin:0;">Миний профайл</h1>
                <button class="m-select" style="cursor:pointer;" onclick="renderCurrentLmsView()">← Буцах</button>
            </div>

            <div class="moodle-card" style="display:flex; align-items:center; gap:24px; flex-wrap:wrap;">
                ${avatarHtml}
                <div style="flex:1;">
                    <span class="grade-pill" style="margin-bottom:8px; display:inline-block;">${roleLabel}</span>
                    <h2 style="margin:0 0 6px; font-size:22px;">${fullName}</h2>
                    <div style="font-size:13.5px; color:var(--muted);">
                        ID / Код: <b>${currentUser.id}</b>  •  ${currentUser.major || 'Дархан Коосэн'}
                    </div>
                </div>
                <button class="btn-primary" onclick="openSettingsModal()">⚙️ Зураг & Нууц үг солих</button>
            </div>

            <!-- ЦАХИМ ҮНЭМЛЭХ (УРД / АР ТАЛ) -->
            <div class="moodle-card">
                <div style="display:flex; justify-content:space-between; align-items:center; flex-wrap:wrap; gap:10px; margin-bottom:16px;">
                    <div class="card-title" style="margin:0;">🪪 Цахим үнэмлэх (${idCardShowBack ? 'Ар тал' : 'Урд тал'})</div>
                    <div style="display:flex; gap:8px;">
                        <button class="btn-primary" style="padding:7px 14px; font-size:12.5px;" onclick="toggleIdCardSide()">
                            🔄 ${idCardShowBack ? 'Урд тал харах' : 'Ар тал харах'}
                        </button>
                        <button class="m-select" style="cursor:pointer; font-size:12.5px;" onclick="printDigitalIdCard()">
                            🖨️ Хэвлэх
                        </button>
                    </div>
                </div>
                <div class="idcard-stage">
                    ${idCardShowBack ? cardBackHtml : cardFrontHtml}
                </div>
            </div>

            <div class="moodle-card">
                <div class="card-title">Хэрэглэгчийн үндсэн мэдээлэл</div>
                <table class="grades-table">
                    <tbody>
                        <tr><td style="width:200px; color:var(--muted);">Бүтэн нэр</td><td><b>${fullName}</b></td></tr>
                        <tr><td style="color:var(--muted);">Системийн ID</td><td><b>${currentUser.id}</b></td></tr>
                        <tr><td style="color:var(--muted);">Албан тушаал / Эрх</td><td>${roleLabel}</td></tr>
                        <tr><td style="color:var(--muted);">Салбар / Мэргэжил</td><td>${currentUser.major || '—'}</td></tr>
                        <tr><td style="color:var(--muted);">Албан ёсны имэйл</td><td>${currentUser.email || '—'}</td></tr>
                    </tbody>
                </table>
            </div>
        </div>
    `;
}

function printDigitalIdCard() {
    if (!currentUser) return;
    const fullName = `${currentUser.lastName || ''} ${currentUser.firstName}`.trim();
    const qrSvg = generateOfflineQrSvg(`KOSEN|${currentUser.id}|${fullName}`);
    printDocument('Цахим үнэмлэхийн хуулбар', `
        <div style="display:flex; gap:24px; align-items:center; border:2px solid #0e4d2d; border-radius:12px; padding:20px; max-width:520px; margin:0 auto;">
            <div>${qrSvg}</div>
            <div style="font-size:14px; line-height:1.7;">
                <div style="font-size:17px; font-weight:800; color:#0e4d2d;">${fullName}</div>
                <div><b>ID / Код:</b> ${currentUser.id}</div>
                <div><b>Салбар:</b> ${currentUser.major || 'Дархан Коосэн'}</div>
                <div><b>Имэйл:</b> ${currentUser.email || '—'}</div>
                <div><b>Хүчинтэй хугацаа:</b> 2026–2027</div>
            </div>
        </div>
    `);
}

// ================= ХЭВЛЭХ / PDF БОЛГОХ (PRINT HELPER) =================
function printDocument(docTitle, contentHtml) {
    let area = document.getElementById('print-area');
    if (!area) {
        area = document.createElement('div');
        area.id = 'print-area';
        document.body.appendChild(area);
    }

    const today = new Date().toISOString().slice(0, 10);
    area.innerHTML = `
        <div class="print-doc">
            <div class="print-header">
                <div>
                    <div style="font-size:12px; color:#475569;">ХӨДӨӨ АЖ АХУЙН ИХ СУРГУУЛИЙН ХАРЬЯА</div>
                    <h2 style="margin:2px 0; color:#0e4d2d;">ДАРХАН КООСЭН ТЕХНОЛОГИЙН КОЛЛЕЖ</h2>
                </div>
                <div style="text-align:right; font-size:12px; color:#475569;">
                    Огноо: ${today}<br>Албан ёсны цахим хуулбар
                </div>
            </div>
            <h3 style="text-align:center; margin:18px 0; text-transform:uppercase; letter-spacing:0.5px;">${docTitle}</h3>
            ${contentHtml}
            <div class="print-footer">
                <div>Хэвлэсэн: ${currentUser ? (currentUser.lastName || '') + ' ' + currentUser.firstName : 'Систем'}</div>
                <div>Тамга / Гарын үсэг: _____________________</div>
            </div>
        </div>
    `;

    window.print();
}

// ================= ХУВИЙН ФАЙЛУУД (PRIVATE FILES - MOODLE ЗАГВАР) =================
let privateFilesList = [];

function openPrivateFiles() {
    closeMenus();
    isGroupChatOpen = false;
    if (!currentUser) return;
    document.querySelectorAll('.lms-link').forEach(l => l.classList.remove('active'));
    postToCSharp({ action: 'getPrivateFiles', userId: currentUser.id });
    renderPrivateFilesPage();
}

function renderPrivateFilesPage() {
    const wrapper = document.getElementById('lms-main-wrapper');
    if (!wrapper || !currentUser) return;

    const avatarHtml = currentUser.avatarUrl
        ? `<img src="${currentUser.avatarUrl}" style="width:68px; height:68px; border-radius:50%; object-fit:cover; border:2px solid var(--green-main);">`
        : `<div class="avatar-circle" style="width:68px; height:68px; font-size:22px;">${getInitials(currentUser.lastName, currentUser.firstName)}</div>`;

    wrapper.innerHTML = `
        <div class="lms-container" style="max-width:960px;">
            <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:20px;">
                <div style="display:flex; align-items:center; gap:16px;">
                    ${avatarHtml}
                    <div>
                        <h1 class="page-heading" style="margin:0;">${currentUser.lastName || ''} ${currentUser.firstName}</h1>
                        <div style="font-size:14px; color:var(--muted);">📁 Хувийн файлууд (Private files)</div>
                    </div>
                </div>
                <button class="m-select" style="cursor:pointer;" onclick="renderCurrentLmsView()">← Буцах</button>
            </div>

            <div class="moodle-card">
                <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:12px;">
                    <div class="card-title" style="margin:0;">Файлууд (${privateFilesList.length})</div>
                    <small style="color:var(--muted);">Дээд хэмжээ: 25 MB • Бүх төрлийн файл</small>
                </div>

                <!-- DRAG & DROP БҮС -->
                <div id="pf-dropzone" class="pf-dropzone"
                     onclick="document.getElementById('pf-file-input').click()"
                     ondragover="event.preventDefault(); this.classList.add('dragover');"
                     ondragleave="this.classList.remove('dragover');"
                     ondrop="handlePrivateFileDrop(event)">
                    <div style="font-size:36px; margin-bottom:8px;">⬇️</div>
                    <div style="font-weight:600; font-size:14.5px;">Файлаа энд чирж тавих эсвэл дарж сонгоно уу</div>
                    <div style="font-size:12px; color:var(--muted); margin-top:4px;">PDF, Word, Excel, Зураг, ZIP гэх мэт</div>
                    <input type="file" id="pf-file-input" style="display:none;" onchange="uploadPrivateFileFromInput(this)">
                </div>

                <!-- ХАДГАЛСАН ФАЙЛУУДЫН ЖАГСААЛТ -->
                <div style="margin-top:18px;">
                    ${privateFilesList.length === 0
            ? `<p style="color:var(--muted); text-align:center; padding:18px;">Одоогоор хадгалсан хувийн файл байхгүй байна.</p>`
            : `<table class="grades-table">
                            <thead><tr><th>Файлын нэр</th><th>Хэмжээ</th><th>Огноо</th><th>Үйлдэл</th></tr></thead>
                            <tbody>
                                ${privateFilesList.map(f => `
                                    <tr>
                                        <td>📄 <b>${f.fileName}</b></td>
                                        <td>${f.fileSize}</td>
                                        <td>${f.uploadedAt}</td>
                                        <td style="white-space:nowrap;">
                                            <a href="${f.fileUrl}" target="_blank" download="${f.fileName}" class="btn-primary" style="padding:4px 10px; font-size:12px; text-decoration:none;">⬇ Татах</a>
                                            <button class="m-select" style="color:#dc2626; cursor:pointer; padding:4px 8px;" onclick="deletePrivateFile(${f.id})">Устгах</button>
                                        </td>
                                    </tr>
                                `).join('')}
                            </tbody>
                        </table>`}
                </div>
            </div>
        </div>
    `;
}

function handlePrivateFileDrop(e) {
    e.preventDefault();
    document.getElementById('pf-dropzone')?.classList.remove('dragover');
    if (e.dataTransfer.files && e.dataTransfer.files[0]) {
        processPrivateFileUpload(e.dataTransfer.files[0]);
    }
}

function uploadPrivateFileFromInput(input) {
    if (input.files && input.files[0]) {
        processPrivateFileUpload(input.files[0]);
        input.value = '';
    }
}

function processPrivateFileUpload(file) {
    if (file.size > 25 * 1024 * 1024) {
        return showToast('Файлын хэмжээ 25MB-аас хэтэрсэн байна.');
    }
    const sizeText = file.size > 1024 * 1024
        ? (file.size / (1024 * 1024)).toFixed(2) + ' MB'
        : Math.max(1, Math.round(file.size / 1024)) + ' KB';

    const reader = new FileReader();
    reader.onload = function (e) {
        postToCSharp({
            action: 'uploadPrivateFile',
            userId: currentUser.id,
            fileName: file.name,
            fileSize: sizeText,
            fileBase64: e.target.result
        });
    };
    reader.readAsDataURL(file);
}

function deletePrivateFile(id) {
    if (confirm('Энэ файлыг устгах уу?')) {
        postToCSharp({ action: 'deletePrivateFile', id, userId: currentUser.id });
    }
}

// ================= ГРУПП ЧАТ (GROUP CHAT) =================
let isGroupChatOpen = false;
let chatGroupsList = [];
let chatAllUsersList = [];
let activeChatGroupId = null;
let activeGroupMessages = [];

function openGroupChatPage() {
    closeMenus();
    if (!currentUser) return;
    isGroupChatOpen = true;
    document.querySelectorAll('.lms-link').forEach(l => l.classList.remove('active'));
    postToCSharp({ action: 'getGroupChats', userId: currentUser.id });
    renderGroupChatPage();
}

function closeGroupChatPage() {
    isGroupChatOpen = false;
    activeChatGroupId = null;
    renderCurrentLmsView();
}

function selectChatGroup(groupId) {
    activeChatGroupId = groupId;
    activeGroupMessages = [];
    postToCSharp({ action: 'getGroupMessages', groupId });
    renderGroupChatPage();
}

function renderGroupChatPage() {
    const wrapper = document.getElementById('lms-main-wrapper');
    if (!wrapper || !currentUser) return;

    const activeGroup = chatGroupsList.find(g => g.id === activeChatGroupId);

    wrapper.innerHTML = `
        <div class="lms-container" style="max-width:1100px;">
            <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:16px;">
                <h1 class="page-heading" style="margin:0;">💬 Групп Чат</h1>
                <button class="m-select" style="cursor:pointer;" onclick="closeGroupChatPage()">← Буцах</button>
            </div>

            <div class="gchat-layout">
                <!-- ЗҮҮН БАГАНА: ГРУППҮҮДИЙН ЖАГСААЛТ -->
                <div class="gchat-sidebar">
                    <button class="btn-primary" style="width:100%; margin-bottom:12px;" onclick="openCreateGroupModal()">
                        + Шинэ групп үүсгэх
                    </button>
                    <div class="gchat-list">
                        ${chatGroupsList.length === 0
            ? `<p style="color:var(--muted); font-size:13px; text-align:center; padding:20px 8px;">Одоогоор групп чат байхгүй байна.<br>Шинэ групп үүсгээрэй!</p>`
            : chatGroupsList.map(g => `
                                <div class="gchat-item ${activeChatGroupId === g.id ? 'active' : ''}" onclick="selectChatGroup(${g.id})">
                                    <div class="gchat-icon">👥</div>
                                    <div style="overflow:hidden;">
                                        <div style="font-weight:700; font-size:13.5px; white-space:nowrap; overflow:hidden; text-overflow:ellipsis;">${g.groupName}</div>
                                        <small style="color:var(--muted);">${g.memberCount} гишүүн</small>
                                    </div>
                                </div>
                            `).join('')}
                    </div>
                </div>

                <!-- БАРУУН БАГАНА: ЧАТЛАХ ЦОНХ -->
                <div class="gchat-main">
                    ${!activeGroup
            ? `<div style="margin:auto; text-align:center; color:var(--muted); padding:40px;">
                               <div style="font-size:44px; margin-bottom:10px;">💬</div>
                               <div>Зүүн талаас групп сонгох эсвэл шинэ групп үүсгэж чат эхлүүлнэ үү.</div>
                           </div>`
            : `
                            <div class="gchat-header">
                                <div>
                                    <div style="font-weight:700; font-size:15.5px;">👥 ${activeGroup.groupName}</div>
                                    <small style="color:var(--muted);">${activeGroup.memberCount} гишүүнтэй групп</small>
                                </div>
                                <button class="m-select" style="cursor:pointer; font-size:12px;" onclick="selectChatGroup(${activeGroup.id})">🔄 Шинэчлэх</button>
                            </div>

                            <div class="gchat-messages" id="gchat-msg-box">
                                ${activeGroupMessages.length === 0
                ? `<p style="color:var(--muted); text-align:center; margin:auto;">Зурвас одоогоор алга. Эхний мэндчилгээгээ бичээрэй!</p>`
                : activeGroupMessages.map(m => {
                    const isMe = m.senderId === currentUser.id;
                    return `
                                            <div class="gchat-msg-row ${isMe ? 'me' : ''}">
                                                <div class="gchat-bubble ${isMe ? 'me' : ''}">
                                                    ${!isMe ? `<div class="gchat-sender">${m.senderName}</div>` : ''}
                                                    <div>${m.messageText}</div>
                                                    <div class="gchat-time">${m.createdAt}</div>
                                                </div>
                                            </div>
                                        `;
                }).join('')}
                            </div>

                            <div class="gchat-input-bar">
                                <input type="text" id="gchat-input" placeholder="Зурвас бичих..."
                                       onkeydown="if(event.key==='Enter') sendGroupMessage()">
                                <button class="btn-primary" onclick="sendGroupMessage()">Илгээх ➤</button>
                            </div>
                        `}
                </div>
            </div>
        </div>
    `;

    const msgBox = document.getElementById('gchat-msg-box');
    if (msgBox) msgBox.scrollTop = msgBox.scrollHeight;
}

function sendGroupMessage() {
    const inp = document.getElementById('gchat-input');
    if (!inp || !activeChatGroupId) return;
    const text = inp.value.trim();
    if (!text) return;

    const senderName = `${currentUser.lastName || ''} ${currentUser.firstName}`.trim();
    postToCSharp({
        action: 'sendGroupMessage',
        groupId: activeChatGroupId,
        senderId: currentUser.id,
        senderName,
        messageText: text
    });
    inp.value = '';
}

function openCreateGroupModal() {
    let modal = document.getElementById('create-group-modal');
    if (!modal) {
        modal = document.createElement('div');
        modal.id = 'create-group-modal';
        modal.className = 'modal-overlay';
        modal.onclick = (e) => { if (e.target === modal) modal.classList.remove('open'); };
        document.body.appendChild(modal);
    }

    const others = chatAllUsersList.filter(u => u.id !== currentUser.id);

    modal.innerHTML = `
        <div class="modal-box" style="max-width:480px; width:92%;">
            <div style="display:flex; justify-content:space-between; align-items:center; border-bottom:1px solid var(--border); padding-bottom:12px; margin-bottom:14px;">
                <h3 style="margin:0; color:var(--green-dark);">👥 Шинэ групп чат үүсгэх</h3>
                <button onclick="document.getElementById('create-group-modal').classList.remove('open')" style="background:none; border:none; font-size:18px; cursor:pointer;">✕</button>
            </div>
            <div class="input-group">
                <label>Группийн нэр:</label>
                <input type="text" id="new-group-name" placeholder="Жнь: IT 4-р курс / Төслийн баг">
            </div>
            <div class="input-group">
                <label>Гишүүд сонгох (${others.length}):</label>
                <div style="max-height:220px; overflow-y:auto; border:1px solid var(--border); border-radius:8px; padding:8px;">
                    ${others.length === 0
            ? `<p style="color:var(--muted); font-size:13px; text-align:center;">Бусад хэрэглэгч олдсонгүй.</p>`
            : others.map(u => `
                            <label style="display:flex; align-items:center; gap:10px; padding:6px 4px; cursor:pointer; font-size:13.5px; border-bottom:1px solid rgba(0,0,0,0.04);">
                                <input type="checkbox" class="gchat-member-cb" value="${u.id}">
                                <span><b>${u.name}</b> <small style="color:var(--muted);">(${u.id} • ${u.roleLabel})</small></span>
                            </label>
                        `).join('')}
                </div>
            </div>
            <div class="modal-footer">
                <button class="m-select" style="cursor:pointer;" onclick="document.getElementById('create-group-modal').classList.remove('open')">Цуцлах</button>
                <button class="btn-primary" onclick="submitCreateGroup()">Үүсгэх</button>
            </div>
        </div>
    `;
    modal.classList.add('open');
}

function submitCreateGroup() {
    const name = document.getElementById('new-group-name').value.trim();
    if (!name) return showToast('Группийн нэрээ оруулна уу.');

    const selected = Array.from(document.querySelectorAll('.gchat-member-cb:checked')).map(cb => cb.value);
    if (!selected.includes(currentUser.id)) selected.push(currentUser.id);

    postToCSharp({
        action: 'createChatGroup',
        groupName: name,
        createdBy: currentUser.id,
        members: selected
    });
    document.getElementById('create-group-modal').classList.remove('open');
}

// ================= ИРЦ БҮРТГЭЛ (БАГШ & ОЮУТАН) =================
let isTeacherAttOpen = false;
let isStudentAttOpen = false;
let attCourseName = '';
let attSelectedDate = new Date().toISOString().slice(0, 10);
let teacherAttList = [];
let studentAttRecords = [];

// 1. БАГШИЙН ИРЦ БҮРТГЭХ ХУУДАС
function openTeacherAttendancePage() {
    closeMenus();
    if (!currentUser || currentUser.role !== 'teacher') return;
    isTeacherAttOpen = true;
    isStudentAttOpen = false;
    isGroupChatOpen = false;
    document.querySelectorAll('.lms-link').forEach(l => l.classList.remove('active'));

    if (!attCourseName && teacherData.courses.length > 0) {
        attCourseName = teacherData.courses[0].name;
    }
    attSelectedDate = new Date().toISOString().slice(0, 10);
    loadTeacherAttendance();
}

function loadTeacherAttendance() {
    if (!attCourseName) {
        renderTeacherAttendancePage();
        return;
    }
    postToCSharp({ action: 'getCourseAttendance', courseName: attCourseName, date: attSelectedDate });
    renderTeacherAttendancePage();
}

function setAttStatus(idx, status) {
    if (teacherAttList[idx]) {
        teacherAttList[idx].status = status;
        renderTeacherAttendancePage();
    }
}

function markAllPresent() {
    teacherAttList.forEach(s => s.status = 'Ирсэн');
    renderTeacherAttendancePage();
}

function saveTeacherAttendance() {
    if (!attCourseName || teacherAttList.length === 0) return;
    const records = teacherAttList.map(s => ({ studentId: s.studentId, status: s.status }));
    postToCSharp({
        action: 'saveAttendance',
        courseName: attCourseName,
        date: attSelectedDate,
        records
    });
}

function renderTeacherAttendancePage() {
    const wrapper = document.getElementById('lms-main-wrapper');
    if (!wrapper) return;

    const statuses = [
        { key: 'Ирсэн', cls: 'active-present', label: '✓ Ирсэн' },
        { key: 'Хоцорсон', cls: 'active-late', label: '⏱ Хоцорсон' },
        { key: 'Тасалсан', cls: 'active-absent', label: '✕ Тасалсан' },
        { key: 'Чөлөөтэй', cls: 'active-excused', label: '📝 Чөлөөтэй' }
    ];

    wrapper.innerHTML = `
        <div class="lms-container" style="max-width:1040px;">
            <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:18px;">
                <h1 class="page-heading" style="margin:0;">📋 Ирц бүртгэл (Багш)</h1>
                <button class="m-select" style="cursor:pointer;" onclick="isTeacherAttOpen=false; renderCurrentLmsView();">← Буцах</button>
            </div>

            <div class="moodle-card">
                <div class="input-row" style="align-items:flex-end;">
                    <div class="input-group" style="flex:2;">
                        <label>Хичээл сонгох:</label>
                        <select onchange="attCourseName=this.value; loadTeacherAttendance();">
                            ${teacherData.courses.length === 0
            ? `<option value="">Хичээл олдсонгүй</option>`
            : teacherData.courses.map(c => `<option value="${c.name}" ${c.name === attCourseName ? 'selected' : ''}>${c.name}</option>`).join('')}
                        </select>
                    </div>
                    <div class="input-group" style="flex:1;">
                        <label>Огноо:</label>
                        <input type="date" value="${attSelectedDate}" onchange="attSelectedDate=this.value; loadTeacherAttendance();">
                    </div>
                    <div style="display:flex; gap:8px; margin-bottom:14px;">
                        <button class="m-select" style="cursor:pointer; padding:10px 14px;" onclick="markAllPresent()">Бүгд ирсэн</button>
                        <button class="btn-primary" style="padding:10px 18px;" onclick="saveTeacherAttendance()">💾 Хадгалах</button>
                    </div>
                </div>
            </div>

            <div class="moodle-card">
                <div class="card-title">Оюутнуудын жагсаалт (${teacherAttList.length}) — ${attSelectedDate}</div>
                ${teacherAttList.length === 0
            ? `<p style="color:var(--muted); text-align:center; padding:24px;">Оюутан олдсонгүй.</p>`
            : `<table class="grades-table">
                        <thead><tr><th>Код</th><th>Овог, Нэр</th><th>Ирцийн төлөв</th></tr></thead>
                        <tbody>
                            ${teacherAttList.map((s, idx) => `
                                <tr>
                                    <td><b>${s.studentId}</b></td>
                                    <td>${s.lastName} ${s.firstName}</td>
                                    <td>
                                        <div style="display:flex; gap:6px; flex-wrap:wrap;">
                                            ${statuses.map(st => `
                                                <button class="att-btn ${s.status === st.key ? st.cls : ''}"
                                                        onclick="setAttStatus(${idx}, '${st.key}')">${st.label}</button>
                                            `).join('')}
                                        </div>
                                    </td>
                                </tr>
                            `).join('')}
                        </tbody>
                    </table>`}
            </div>
        </div>
    `;
}

// 2. ОЮУТНЫ ИРЦ ХАРАХ ХУУДАС
function openStudentAttendancePage() {
    closeMenus();
    if (!currentUser) return;
    isStudentAttOpen = true;
    isTeacherAttOpen = false;
    isGroupChatOpen = false;
    document.querySelectorAll('.lms-link').forEach(l => l.classList.remove('active'));
    postToCSharp({ action: 'getStudentAttendance', studentId: currentUser.id });
    renderStudentAttendancePage();
}

function renderStudentAttendancePage() {
    const wrapper = document.getElementById('lms-main-wrapper');
    if (!wrapper) return;

    const total = studentAttRecords.length;
    const present = studentAttRecords.filter(r => r.status === 'Ирсэн').length;
    const late = studentAttRecords.filter(r => r.status === 'Хоцорсон').length;
    const excused = studentAttRecords.filter(r => r.status === 'Чөлөөтэй').length;
    const absent = studentAttRecords.filter(r => r.status === 'Тасалсан').length;

    const percent = total === 0 ? 100 : Math.round(((present + late + excused) / total) * 100);
    const isWarning = total > 0 && percent < 70;

    const badgeColor = (st) => {
        if (st === 'Ирсэн') return 'background:#dcfce7; color:#15803d;';
        if (st === 'Хоцорсон') return 'background:#fef9c3; color:#a16207;';
        if (st === 'Тасалсан') return 'background:#fee2e2; color:#dc2626;';
        return 'background:#e0f2fe; color:#0369a1;';
    };

    wrapper.innerHTML = `
        <div class="lms-container" style="max-width:960px;">
            <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:18px;">
                <h1 class="page-heading" style="margin:0;">📋 Миний ирцийн бүртгэл</h1>
                <button class="m-select" style="cursor:pointer;" onclick="isStudentAttOpen=false; renderCurrentLmsView();">← Буцах</button>
            </div>

            ${isWarning ? `
                <div class="moodle-card" style="border-left:5px solid #dc2626; background:#fef2f2 !important; color:#991b1b;">
                    <b>⚠️ Анхааруулга:</b> Таны нийт ирц <b>${percent}%</b> буюу 70%-аас доош орсон байна! Шалгалтад орох эрх хаагдах эрсдэлтэй тул хичээлдээ тогтмол сууна уу.
                </div>
            ` : ''}

            <div class="moodle-card" style="display:grid; grid-template-columns:repeat(auto-fit, minmax(150px, 1fr)); gap:12px; text-align:center;">
                <div style="padding:12px; border-radius:10px; background:var(--surface-2);">
                    <div style="font-size:26px; font-weight:800; color:${isWarning ? '#dc2626' : 'var(--accent)'};">${percent}%</div>
                    <div style="font-size:12px; color:var(--muted);">Нийт ирцийн хувь</div>
                </div>
                <div style="padding:12px; border-radius:10px; background:var(--surface-2);">
                    <div style="font-size:24px; font-weight:800; color:#16a34a;">${present}</div>
                    <div style="font-size:12px; color:var(--muted);">Ирсэн</div>
                </div>
                <div style="padding:12px; border-radius:10px; background:var(--surface-2);">
                    <div style="font-size:24px; font-weight:800; color:#ca8a04;">${late}</div>
                    <div style="font-size:12px; color:var(--muted);">Хоцорсон</div>
                </div>
                <div style="padding:12px; border-radius:10px; background:var(--surface-2);">
                    <div style="font-size:24px; font-weight:800; color:#dc2626;">${absent}</div>
                    <div style="font-size:12px; color:var(--muted);">Тасалсан</div>
                </div>
            </div>

            <div class="moodle-card">
                <div class="card-title">Ирцийн дэлгэрэнгүй түүх (${total})</div>
                ${total === 0
            ? `<p style="color:var(--muted); text-align:center; padding:24px;">Одоогоор ирцийн бүртгэл ороогүй байна.</p>`
            : `<table class="grades-table">
                        <thead><tr><th>Огноо</th><th>Хичээлийн нэр</th><th>Төлөв</th></tr></thead>
                        <tbody>
                            ${studentAttRecords.map(r => `
                                <tr>
                                    <td><b>${r.date}</b></td>
                                    <td>${r.courseName}</td>
                                    <td><span class="grade-pill" style="${badgeColor(r.status)}">${r.status}</span></td>
                                </tr>
                            `).join('')}
                        </tbody>
                    </table>`}
            </div>
        </div>
    `;
}

// ================= ДААЛГАВАР ИЛГЭЭХ & ҮНЭЛЭХ (ASSIGNMENTS) =================
let isAssignmentsOpen = false;
let submissionsList = [];
let asgFilterCourse = '';

function openAssignmentsPage() {
    closeMenus();
    if (!currentUser) return;
    isAssignmentsOpen = true;
    isTeacherAttOpen = false;
    isStudentAttOpen = false;
    isGroupChatOpen = false;
    document.querySelectorAll('.lms-link').forEach(l => l.classList.remove('active'));

    postToCSharp({
        action: 'getSubmissions',
        role: currentUser.role,
        userId: currentUser.id,
        courseName: asgFilterCourse
    });
    renderAssignmentsPage();
}

function renderAssignmentsPage() {
    const wrapper = document.getElementById('lms-main-wrapper');
    if (!wrapper || !currentUser) return;

    // 1. БАГШИЙН ХАРАГДАЦ (ИРСЭН ДААЛГАВРУУДЫГ ШАЛГАЖ ҮНЭЛЭХ)
    if (currentUser.role === 'teacher') {
        wrapper.innerHTML = `
            <div class="lms-container" style="max-width:1100px;">
                <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:18px;">
                    <h1 class="page-heading" style="margin:0;">📝 Ирсэн даалгаврууд (Үнэлгээ)</h1>
                    <button class="m-select" style="cursor:pointer;" onclick="isAssignmentsOpen=false; renderCurrentLmsView();">← Буцах</button>
                </div>

                <div class="moodle-card" style="display:flex; justify-content:space-between; align-items:center; flex-wrap:wrap; gap:12px;">
                    <div style="display:flex; align-items:center; gap:10px;">
                        <label style="font-weight:600; font-size:13.5px;">Хичээлээр шүүх:</label>
                        <select class="m-select" onchange="asgFilterCourse=this.value; openAssignmentsPage();">
                            <option value="">Бүх хичээл</option>
                            ${teacherData.courses.map(c => `<option value="${c.name}" ${c.name === asgFilterCourse ? 'selected' : ''}>${c.name}</option>`).join('')}
                        </select>
                    </div>
                    <button class="m-select" style="cursor:pointer;" onclick="openAssignmentsPage()">🔄 Шинэчлэх</button>
                </div>

                <div class="moodle-card">
                    <div class="card-title">Оюутнуудын илгээсэн ажлууд (${submissionsList.length})</div>
                    ${submissionsList.length === 0
                ? `<p style="color:var(--muted); text-align:center; padding:24px;">Одоогоор илгээсэн даалгавар байхгүй байна.</p>`
                : `<table class="grades-table">
                            <thead><tr><th>Оюутан</th><th>Хичээл / Тайлбар</th><th>Файл</th><th>Оноо & Санал</th><th>Үйлдэл</th></tr></thead>
                            <tbody>
                                ${submissionsList.map(s => `
                                    <tr>
                                        <td><b>${s.studentName}</b><br><small style="color:var(--muted);">${s.studentId} • ${s.submittedAt}</small></td>
                                        <td><b>${s.courseName}</b><br><small style="color:var(--muted);">${s.comment || 'Тайлбаргүй'}</small></td>
                                        <td>
                                            <a href="${s.fileUrl}" target="_blank" download="${s.fileName}" class="m-select" style="text-decoration:none; display:inline-block; font-size:12px;">
                                                ⬇ ${s.fileName}
                                            </a>
                                        </td>
                                        <td>
                                            <div style="display:flex; gap:6px; flex-wrap:wrap;">
                                                <input type="number" id="asg-sc-${s.id}" min="0" max="100" placeholder="0-100"
                                                       value="${s.score !== null && s.score !== undefined ? s.score : ''}"
                                                       style="width:72px; padding:6px; border:1px solid var(--border); border-radius:6px;">
                                                <input type="text" id="asg-fb-${s.id}" placeholder="Багшийн тайлбар..."
                                                       value="${(s.feedback || '').replace(/"/g, '"')}"
                                                       style="width:160px; padding:6px; border:1px solid var(--border); border-radius:6px;">
                                            </div>
                                        </td>
                                        <td>
                                            <button class="btn-primary" style="padding:6px 12px; font-size:12px;" onclick="saveSubmissionGrade(${s.id})">
                                                ${s.score !== null && s.score !== undefined ? '✓ Засах' : 'Үнэлэх'}
                                            </button>
                                        </td>
                                    </tr>
                                `).join('')}
                            </tbody>
                        </table>`}
                </div>
            </div>
        `;
        return;
    }

    // 2. ОЮУТНЫ ХАРАГДАЦ (ДААЛГАВАР ИЛГЭЭХ + ДҮНГЭЭ ХАРАХ)
    const myCourses = studentData.enrolled.length > 0 ? studentData.enrolled : studentData.available;

    wrapper.innerHTML = `
        <div class="lms-container" style="max-width:980px;">
            <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:18px;">
                <h1 class="page-heading" style="margin:0;">📤 Даалгавар илгээх (Assignments)</h1>
                <button class="m-select" style="cursor:pointer;" onclick="isAssignmentsOpen=false; renderCurrentLmsView();">← Буцах</button>
            </div>

            <div class="moodle-card">
                <div class="card-title">Шинэ даалгавар / бие даалт илгээх</div>
                <div class="input-row">
                    <div class="input-group" style="flex:1;">
                        <label>Хичээл сонгох:</label>
                        <select id="stu-asg-course">
                            ${myCourses.map(c => `<option value="${c.name}">${c.name}</option>`).join('')}
                        </select>
                    </div>
                    <div class="input-group" style="flex:1;">
                        <label>Даалгаврын файл (PDF, DOCX, ZIP, Зураг):</label>
                        <input type="file" id="stu-asg-file">
                    </div>
                </div>
                <div class="input-group">
                    <label>Тайлбар (Даалгаврын нэр эсвэл тэмдэглэл):</label>
                    <input type="text" id="stu-asg-comment" placeholder="Жнь: Лабораторийн ажил №3 — Н.Номинхаш">
                </div>
                <button class="btn-primary" onclick="sendStudentAssignment()">📤 Илгээх</button>
            </div>

            <div class="moodle-card">
                <div class="card-title">Миний илгээсэн даалгаврууд (${submissionsList.length})</div>
                ${submissionsList.length === 0
            ? `<p style="color:var(--muted); text-align:center; padding:24px;">Та одоогоор даалгавар илгээгээгүй байна.</p>`
            : `<table class="grades-table">
                        <thead><tr><th>Огноо</th><th>Хичээл & Тайлбар</th><th>Файл</th><th>Үнэлгээ</th><th>Багшийн санал</th></tr></thead>
                        <tbody>
                            ${submissionsList.map(s => `
                                <tr>
                                    <td><small>${s.submittedAt}</small></td>
                                    <td><b>${s.courseName}</b><br><small style="color:var(--muted);">${s.comment || '—'}</small></td>
                                    <td><a href="${s.fileUrl}" target="_blank" download="${s.fileName}" style="color:var(--accent); font-weight:600;">📄 ${s.fileName}</a></td>
                                    <td>
                                        ${s.score !== null && s.score !== undefined
                    ? `<span class="grade-pill" style="background:#dcfce7; color:#15803d;">${s.score} / 100</span>`
                    : `<span class="grade-pill">Хүлээгдэж буй</span>`}
                                    </td>
                                    <td>${s.feedback || '—'}</td>
                                </tr>
                            `).join('')}
                        </tbody>
                    </table>`}
            </div>
        </div>
    `;
}

function sendStudentAssignment() {
    const courseName = document.getElementById('stu-asg-course')?.value || '';
    const fileInput = document.getElementById('stu-asg-file');
    const comment = document.getElementById('stu-asg-comment')?.value.trim() || '';

    if (!courseName) return showToast('Хичээлээ сонгоно уу.');
    if (!fileInput || !fileInput.files || !fileInput.files[0]) {
        return showToast('Даалгаврын файлаа сонгоно уу.');
    }

    const file = fileInput.files[0];
    if (file.size > 25 * 1024 * 1024) {
        return showToast('Файлын хэмжээ 25MB-аас хэтэрсэн байна.');
    }

    const reader = new FileReader();
    reader.onload = function (e) {
        postToCSharp({
            action: 'submitAssignment',
            materialId: 0,
            courseName,
            studentId: currentUser.id,
            fileName: file.name,
            fileBase64: e.target.result,
            comment
        });
    };
    reader.readAsDataURL(file);
}

function saveSubmissionGrade(id) {
    const scVal = document.getElementById(`asg-sc-${id}`)?.value;
    const fbVal = document.getElementById(`asg-fb-${id}`)?.value.trim() || '';
    const score = parseInt(scVal, 10);

    if (isNaN(score) || score < 0 || score > 100) {
        return showToast('0-ээс 100-ийн хооронд оноо оруулна уу.');
    }

    postToCSharp({
        action: 'gradeSubmission',
        id,
        score,
        feedback: fbVal
    });
}

// ================= ДОЛОО ХОНОГИЙН ХИЧЭЭЛИЙН ХУВААРЬ (WEEKLY TIMETABLE) =================
let isTimetableOpen = false;
let timetableSlots = [];
let ttFilterMineOnly = false;

function openTimetablePage() {
    closeMenus();
    if (!currentUser) return;
    isTimetableOpen = true;
    isAssignmentsOpen = false;
    isTeacherAttOpen = false;
    isStudentAttOpen = false;
    isGroupChatOpen = false;
    document.querySelectorAll('.lms-link').forEach(l => l.classList.remove('active'));

    postToCSharp({ action: 'getTimetable' });
    renderTimetablePage();
}

function renderTimetablePage() {
    const wrapper = document.getElementById('lms-main-wrapper');
    if (!wrapper || !currentUser) return;

    const canEdit = (currentUser.role === 'teacher' || currentUser.role === 'admin');
    const todayIdx = new Date().getDay(); // 1=Даваа ... 5=Баасан
    const days = [
        { num: 1, name: 'Даваа' },
        { num: 2, name: 'Мягмар' },
        { num: 3, name: 'Лхагва' },
        { num: 4, name: 'Пүрэв' },
        { num: 5, name: 'Баасан' }
    ];

    // Оюутан бол өөрийн сонгосон хичээлээр шүүх боломжтой
    let visibleSlots = timetableSlots;
    if (currentUser.role === 'student' && ttFilterMineOnly) {
        const myNames = studentData.enrolled.map(c => c.name);
        visibleSlots = timetableSlots.filter(s => myNames.includes(s.courseName));
    }

    // Багш / Админ шинэ цаг нэмэхэд ашиглах хичээлийн жагсаалт
    const allCourses = currentUser.role === 'teacher' ? teacherData.courses
        : currentUser.role === 'admin' ? adminData.courses : [];

    const typeStyle = (tp) => {
        if (tp === 'Лаборатори') return 'background:#e0f2fe; color:#0369a1;';
        if (tp === 'Семинар') return 'background:#fef3c7; color:#b45309;';
        return 'background:#dcfce7; color:#15803d;';
    };

    wrapper.innerHTML = `
        <div class="lms-container" style="max-width:1180px;">
            <div style="display:flex; justify-content:space-between; align-items:center; flex-wrap:wrap; gap:10px; margin-bottom:18px;">
                <h1 class="page-heading" style="margin:0;">🗓️ Долоо хоногийн хичээлийн хуваарь</h1>
                <div style="display:flex; gap:8px; flex-wrap:wrap;">
                    ${currentUser.role === 'student' ? `
                        <button class="m-select" style="cursor:pointer; font-weight:600;" onclick="ttFilterMineOnly=!ttFilterMineOnly; renderTimetablePage();">
                            ${ttFilterMineOnly ? '📚 Бүх хуваарь харах' : '🎯 Зөвхөн миний хичээлүүд'}
                        </button>
                    ` : ''}
                    <button class="m-select" style="cursor:pointer;" onclick="printTimetable()">🖨️ Хэвлэх</button>
                    <button class="m-select" style="cursor:pointer;" onclick="isTimetableOpen=false; renderCurrentLmsView();">← Буцах</button>
                </div>
            </div>

            ${canEdit ? `
                <div class="moodle-card" style="margin-bottom:18px;">
                    <div class="card-title">+ Хуваарьт шинэ цаг нэмэх</div>
                    <div class="input-row">
                        <div class="input-group" style="flex:1;">
                            <label>Гариг:</label>
                            <select id="tt-day">
                                ${days.map(d => `<option value="${d.num}">${d.name}</option>`).join('')}
                            </select>
                        </div>
                        <div class="input-group" style="flex:1;">
                            <label>Цагийн хуваарь:</label>
                            <select id="tt-time">
                                <option value="08:30 - 10:00">1-р пар (08:30 - 10:00)</option>
                                <option value="10:15 - 11:45">2-р пар (10:15 - 11:45)</option>
                                <option value="12:30 - 14:00">3-р пар (12:30 - 14:00)</option>
                                <option value="14:15 - 15:45">4-р пар (14:15 - 15:45)</option>
                                <option value="16:00 - 17:30">5-р пар (16:00 - 17:30)</option>
                            </select>
                        </div>
                        <div class="input-group" style="flex:2;">
                            <label>Хичээл:</label>
                            ${allCourses.length > 0
                ? `<select id="tt-course">${allCourses.map(c => `<option value="${c.name}">${c.name}</option>`).join('')}</select>`
                : `<input type="text" id="tt-course" placeholder="Хичээлийн нэр...">`}
                        </div>
                    </div>
                    <div class="input-row" style="align-items:flex-end;">
                        <div class="input-group" style="flex:1;">
                            <label>Анги / Танхим:</label>
                            <input type="text" id="tt-room" placeholder="Жнь: Lab-204">
                        </div>
                        <div class="input-group" style="flex:1;">
                            <label>Багшийн нэр:</label>
                            <input type="text" id="tt-teacher" value="${currentUser.role === 'teacher' ? currentUser.firstName : ''}" placeholder="Багшийн нэр...">
                        </div>
                        <div class="input-group" style="flex:1;">
                            <label>Хичээлийн төрөл:</label>
                            <select id="tt-type">
                                <option value="Лекц">Лекц</option>
                                <option value="Лаборатори">Лаборатори</option>
                                <option value="Семинар">Семинар</option>
                            </select>
                        </div>
                        <div style="margin-bottom:14px;">
                            <button class="btn-primary" style="padding:10px 18px;" onclick="submitTimetableSlot()">+ Нэмэх</button>
                        </div>
                    </div>
                </div>
            ` : ''}

            <div class="tt-grid">
                ${days.map(d => {
                    const daySlots = visibleSlots.filter(s => s.dayOfWeek === d.num);
                    const isToday = (todayIdx === d.num);
                    return `
                        <div class="tt-day-col ${isToday ? 'today' : ''}">
                            <div class="tt-day-header">
                                <span>${d.name}</span>
                                ${isToday ? `<span class="tt-today-tag">Өнөөдөр</span>` : `<small style="opacity:0.65;">${daySlots.length} пар</small>`}
                            </div>
                            <div class="tt-day-body">
                                ${daySlots.length === 0
                            ? `<div class="tt-empty">Хичээлгүй</div>`
                            : daySlots.map(s => `
                                        <div class="tt-card">
                                            <div style="display:flex; justify-content:space-between; align-items:center; gap:6px; margin-bottom:6px;">
                                                <span class="tt-time">🕒 ${s.timeSlot}</span>
                                                <span class="tt-type-badge" style="${typeStyle(s.courseType)}">${s.courseType}</span>
                                            </div>
                                            <div class="tt-course">${s.courseName}</div>
                                            <div class="tt-meta">📍 ${s.room || '—'}</div>
                                            <div class="tt-meta">👨‍🏫 ${s.teacherName || '—'}</div>
                                            ${canEdit ? `<button class="tt-del-btn" onclick="deleteTimetableSlot(${s.id})" title="Устгах">✕</button>` : ''}
                                        </div>
                                    `).join('')}
                            </div>
                        </div>
                    `;
                }).join('')}
            </div>
        </div>
    `;
}

function submitTimetableSlot() {
    const dayOfWeek = parseInt(document.getElementById('tt-day')?.value || '1', 10);
    const timeSlot = document.getElementById('tt-time')?.value || '08:30 - 10:00';
    const courseName = document.getElementById('tt-course')?.value.trim() || '';
    const room = document.getElementById('tt-room')?.value.trim() || '301 тоот';
    const teacherName = document.getElementById('tt-teacher')?.value.trim() || '';
    const courseType = document.getElementById('tt-type')?.value || 'Лекц';

    if (!courseName) return showToast('Хичээлийн нэрээ сонгоно уу.');

    postToCSharp({
        action: 'addTimetableSlot',
        dayOfWeek,
        timeSlot,
        courseName,
        room,
        teacherName,
        courseType
    });
}

function deleteTimetableSlot(id) {
    if (confirm('Энэ цагийн хуваарийг устгах уу?')) {
        postToCSharp({ action: 'deleteTimetableSlot', id });
    }
}

function printTimetable() {
    const dayNames = { 1: 'Даваа', 2: 'Мягмар', 3: 'Лхагва', 4: 'Пүрэв', 5: 'Баасан' };
    const rows = timetableSlots.map(s => `
        <tr>
            <td><b>${dayNames[s.dayOfWeek] || ''}</b></td>
            <td>${s.timeSlot}</td>
            <td><b>${s.courseName}</b></td>
            <td>${s.courseType}</td>
            <td>${s.room}</td>
            <td>${s.teacherName}</td>
        </tr>
    `).join('');

    printDocument('Долоо хоногийн хичээлийн хуваарь', `
        <table class="grades-table" style="width:100%; border-collapse:collapse;">
            <thead><tr><th>Гариг</th><th>Цаг</th><th>Хичээл</th><th>Төрөл</th><th>Анги</th><th>Багш</th></tr></thead>
            <tbody>${rows || '<tr><td colspan="6" style="text-align:center;">Хуваарь ороогүй байна.</td></tr>'}</tbody>
        </table>
    `);
}

// ================= КОСЭН МАСКОТ (MASCOT WIDGET) =================
(function initKosenMascot() {
    const tips = [
        'Дархан Коосэн цахим системд тавтай морил! 🎓',
        '🗓️ Хичээлийн хуваарь болон ирцээ баруун дээд цэснээс шалгаарай.',
        '🪪 Профайл руу орж өөрийн Цахим үнэмлэхээ хараарай!',
        '🎨 Тохиргоо хэсгээс 4 төрлийн өнгөний загвараас сонгох боломжтой.',
        '📤 Даалгавраа цаг тухайд нь PDF/ZIP файлаар илгээгээрэй!'
    ];
    let tipIdx = 0;

    function mountMascot() {
        if (document.getElementById('kosen-mascot-wrap')) return;
        const wrap = document.createElement('div');
        wrap.id = 'kosen-mascot-wrap';
        wrap.className = 'kosen-mascot-wrap';
        wrap.innerHTML = `
            <div class="kosen-mascot-bubble" id="kosen-mascot-bubble">
                <button class="kosen-mascot-close" onclick="toggleMascotMinimize(event)" title="Нуух">✕</button>
                <div id="kosen-mascot-text" style="padding-right:12px;">${tips[0]}</div>
            </div>
            <button class="kosen-mascot-img-btn" onclick="onMascotClick()" title="Дархан Коосэн Маскот">
                <img src="images/mascot.png" alt="Mascot">
            </button>
        `;
        document.body.appendChild(wrap);

        // 7 секундийн дараа мессежийн бөмбөлгийг автоматаар далдлах
        setTimeout(() => {
            document.getElementById('kosen-mascot-bubble')?.classList.add('hidden');
        }, 7000);
    }

    window.onMascotClick = function () {
        const wrap = document.getElementById('kosen-mascot-wrap');
        const bubble = document.getElementById('kosen-mascot-bubble');
        const txt = document.getElementById('kosen-mascot-text');
        if (!wrap || !bubble || !txt) return;

        if (wrap.classList.contains('minimized')) {
            wrap.classList.remove('minimized');
            bubble.classList.remove('hidden');
            return;
        }

        tipIdx = (tipIdx + 1) % tips.length;
        const greeting = (typeof currentUser !== 'undefined' && currentUser && tipIdx === 0)
            ? `Сайн байна уу, ${currentUser.firstName}! Өнөөдөр амжилттай суралцаарай 🎓`
            : tips[tipIdx];

        txt.innerText = greeting;
        bubble.classList.remove('hidden');
    };

    window.toggleMascotMinimize = function (e) {
        e.stopPropagation();
        document.getElementById('kosen-mascot-bubble')?.classList.add('hidden');
        document.getElementById('kosen-mascot-wrap')?.classList.add('minimized');
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', mountMascot);
    } else {
        mountMascot();
    }
})();