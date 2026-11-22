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
            <div class="dd-item" onclick="switchLmsTab('tuition')">💳 Сургалтын төлбөр</div>
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

// ================= БҮХ ЭРХЭД АЖИЛЛАХ ПРОФАЙЛ ХУУДАС =================
function openUniversalProfile() {
    closeMenus();
    isGroupChatOpen = false;
    if (!currentUser) return;

    document.querySelectorAll('.lms-link').forEach(l => l.classList.remove('active'));
    const wrapper = document.getElementById('lms-main-wrapper');
    if (!wrapper) return;

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

    const avatarHtml = currentUser.avatarUrl
        ? `<img src="${currentUser.avatarUrl}" style="width:96px; height:96px; border-radius:50%; object-fit:cover; border:3px solid var(--green-main);">`
        : `<div class="avatar-circle" style="width:96px; height:96px; font-size:30px;">${getInitials(currentUser.lastName, currentUser.firstName)}</div>`;

    wrapper.innerHTML = `
        <div class="lms-container" style="max-width:820px;">
            <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:18px;">
                <h1 class="page-heading" style="margin:0;">Миний профайл</h1>
                <button class="m-select" style="cursor:pointer;" onclick="renderCurrentLmsView()">← Буцах</button>
            </div>

            <div class="moodle-card" style="display:flex; align-items:center; gap:24px; flex-wrap:wrap;">
                ${avatarHtml}
                <div style="flex:1;">
                    <span class="grade-pill" style="margin-bottom:8px; display:inline-block;">${roleLabel}</span>
                    <h2 style="margin:0 0 6px; font-size:22px;">${currentUser.lastName || ''} ${currentUser.firstName}</h2>
                    <div style="font-size:13.5px; color:var(--muted);">
                        ID / Код: <b>${currentUser.id}</b>  •  ${currentUser.major || 'Дархан Коосэн'}
                    </div>
                </div>
                <button class="btn-primary" onclick="openSettingsModal()">⚙️ Зураг & Нууц үг солих</button>
            </div>

            <div class="moodle-card">
                <div class="card-title">Хэрэглэгчийн үндсэн мэдээлэл</div>
                <table class="grades-table">
                    <tbody>
                        <tr><td style="width:200px; color:var(--muted);">Бүтэн нэр</td><td><b>${currentUser.lastName || ''} ${currentUser.firstName}</b></td></tr>
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