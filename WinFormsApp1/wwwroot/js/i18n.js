let currentLang = 'mn';

const I18N = {
    mn: {
        login_btn: 'Системд нэвтрэх ➔',
        pub_home: 'НҮҮР',
        pub_lms: 'ОЮУТНЫ LMS СИСТЕМ',
        hero_title: 'Ногоон их сургууль',
        hero_desc: 'Япон стандартын 5 жилийн инженерийн нэгдсэн хөтөлбөр. Хичээл сонголт, цахим сургалт болон дүнгийн мэдээллээ дээрх "Системд нэвтрэх" товчоор орж харна уу.',
        login_sub: 'Сургалтын удирдлагын системд нэвтрэх ↴',
        role_student: 'Оюутан',
        role_teacher: 'Багш',
        role_admin: 'Админ',
        id_student: 'Оюутны код:',
        id_teacher: 'Багшийн ID:',
        id_admin: 'Админ нэвтрэх нэр:',
        password: 'Нууц үг:',
        login_submit: 'Системд нэвтрэх',
        back_home: '← Нүүр хуудас руу буцах',
        nav_home: 'Нүүр хуудас',
        nav_dashboard: 'Удирдах самбар',
        nav_courses: 'Миний хичээлүүд',
        edit_mode: 'Горимыг засварлах',
        notifications: 'Мэдэгдлүүд',
        see_all: 'Бүгдийг харах',
        menu_profile: 'Профайл',
        menu_grades: 'Дүнгийн жагсаалт',
        menu_calendar: 'Календарь',
        menu_files: 'Хувийн файлууд',
        menu_reports: 'Тайлан',
        menu_prefs: 'Тохиргоо',
        menu_lang: 'Хэл',
        menu_logout: 'Гарах',
        site_announcements: 'Сайтын зарлал',
        subscribe_forum: 'Форумд бүртгүүлэх',
        no_topics: 'Энэ форумд одоогоор хэлэлцүүлэг байхгүй байна.',
        search_courses: 'Хичээл хайх',
        available_courses: 'Боломжит хичээлүүд',
        enrolled: '✓ Сонгосон',
        enroll: '+ Сонгох',
        hi: 'Сайн байна уу',
        timeline: 'Цагийн хуваарь',
        next_30: 'Дараагийн 30 хоног',
        next_7: 'Дараагийн 7 хоног',
        all: 'Бүгд',
        sort_dates: 'Огноогоор хайх',
        sort_courses: 'Хичээлээр эрэмбэлэх',
        search_activity: 'Үйл ажиллагааны төрөл эсвэл нэрээр хайх',
        no_activities: 'Үйлдэл шаардсан үйл ажиллагаа байхгүй байна',
        calendar: 'Календарь',
        all_courses: 'Бүх хичээл',
        new_event: 'Шинэ арга хэмжээ',
        ev_title: 'Арга хэмжээний нэр:',
        ev_course: 'Хичээл:',
        ev_date: 'Огноо:',
        ev_time: 'Цаг:',
        cancel: 'Цуцлах',
        save: 'Хадгалах',
        go_to_activity: 'Үйл ажиллагаа руу очих',
        course_overview: 'Хичээлийн тойм',
        sort_name: 'Нэрээр эрэмбэлэх',
        card_view: 'Карт',
        list_view: 'Жагсаалт',
        feedback_title: 'Санал хүсэлт',
        feedback_desc: 'Moodle системийн талаарх санал хүсэлтээ admin@sab.edu.mn хаягаар илгээнэ үү.',
        months: ['Нэгдүгээр сар', 'Хоёрдугаар сар', 'Гуравдугаар сар', 'Дөрөвдүгээр сар', 'Тавдугаар сар', 'Зургаадугаар сар', 'Долоодугаар сар', 'Наймдугаар сар', 'Есдүгээр сар', 'Аравдугаар сар', 'Арван нэгдүгээр сар', 'Арван хоёрдугаар сар'],
        weekdays: ['Да', 'Мя', 'Лха', 'Пү', 'Ба', 'Бя', 'Ня'],
        pub_sub: 'ХӨДӨӨ АЖ АХУЙН ИХ СУРГУУЛИЙН ДАРХАН-УУЛ АЙМАГ ДАХЬ',
        pub_title: 'ДАРХАН КООСЭН ТЕХНОЛОГИЙН КОЛЛЕЖ',
        btn_elselt: 'ЭЛСЭЛТИЙН БҮРТГЭЛ ▸',
        btn_portal: 'СИСТЕМД НЭВТРЭХ ▸',
        nav_p_home: '🏠 НҮҮР',
        nav_p_about: 'БИДНИЙ ТУХАЙ ▾',
        nav_p_prog: 'ХӨТӨЛБӨР ▾',
        nav_p_edu: 'СУРГАЛТЫН ҮЙЛ АЖИЛЛАГАА ▾',
        nav_p_intl: 'ГАДААД ХАРИЛЦАА ▾',
        nav_p_contact: 'ХОЛБОО БАРИХ',
        kosen_slogan: '15 НАСТАЙГААС 20 НАС ХҮРТЭЛ 5 ЖИЛИЙН ЦОГЦ ХӨТӨЛБӨРӨӨР ИНЖЕНЕР БЭЛТГЭДЭГ ЯПОН ЗАГВАРЫН СУРГУУЛЬ',
        quota_full: 'ДҮҮРСЭН',
        quota_seats: 'Суудал',
        ticker_badge: 'ЦӨӨН МӨРӨӨР',
        featured_news: 'ТЭРГҮҮН МЭДЭЭ',
        sec_news: 'МЭДЭЭ',
        sec_announce: 'ЗАРЛАЛ',
        footer_contact: 'ХОЛБОО БАРИХ',
        footer_links: 'ЧУХАЛ ХОЛБООСУУД',
        footer_copy: '© 2026 ХААИС-ийн харьяа Дархан Коосэн Технологийн Коллеж. Бүх эрх хуулиар хамгаалагдсан.',
    },
    en: {
        login_btn: 'Log in to LMS ➔',
        pub_home: 'HOME',
        pub_lms: 'STUDENT LMS PORTAL',
        hero_title: 'Green University',
        hero_desc: 'Japanese-standard 5-year integrated engineering program. Access your courses, e-learning materials, and grades via the "Log in to LMS" button above.',
        login_sub: 'Log in to Learning Management System ↴',
        role_student: 'Student',
        role_teacher: 'Teacher',
        role_admin: 'Admin',
        id_student: 'Student ID:',
        id_teacher: 'Teacher ID:',
        id_admin: 'Admin Username:',
        password: 'Password:',
        login_submit: 'Log in',
        back_home: '← Back to Home',
        nav_home: 'Home',
        nav_dashboard: 'Dashboard',
        nav_courses: 'My courses',
        edit_mode: 'Edit mode',
        notifications: 'Notifications',
        see_all: 'See all',
        menu_profile: 'Profile',
        menu_grades: 'Grades',
        menu_calendar: 'Calendar',
        menu_files: 'Private files',
        menu_reports: 'Reports',
        menu_prefs: 'Preferences',
        menu_lang: 'Language',
        menu_logout: 'Log out',
        site_announcements: 'Site announcements',
        subscribe_forum: 'Subscribe to forum',
        no_topics: 'There are no discussion topics yet in this forum.',
        search_courses: 'Search courses',
        available_courses: 'Available courses',
        enrolled: '✓ Enrolled',
        enroll: '+ Enroll',
        hi: 'Hi',
        timeline: 'Timeline',
        next_30: 'Next 30 days',
        next_7: 'Next 7 days',
        all: 'All',
        sort_dates: 'Sort by dates',
        sort_courses: 'Sort by courses',
        search_activity: 'Search by activity type or name',
        no_activities: 'No activities require action',
        calendar: 'Calendar',
        all_courses: 'All courses',
        new_event: 'New event',
        ev_title: 'Event title:',
        ev_course: 'Course:',
        ev_date: 'Date:',
        ev_time: 'Time:',
        cancel: 'Cancel',
        save: 'Save',
        go_to_activity: 'Go to activity',
        course_overview: 'Course overview',
        sort_name: 'Sort by course name',
        card_view: 'Card',
        list_view: 'List',
        feedback_title: 'Feedback',
        feedback_desc: 'Please send your feedback regarding the Moodle system to admin@sab.edu.mn.',
        months: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
        weekdays: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
        pub_sub: 'MONGOLIAN UNIVERSITY OF LIFE SCIENCES • DARKHAN-UUL',
        pub_title: 'DARKHAN KOSEN COLLEGE OF TECHNOLOGY',
        btn_elselt: 'ADMISSIONS ▸',
        btn_portal: 'SYSTEM LOGIN ▸',
        nav_p_home: '🏠 HOME',
        nav_p_about: 'ABOUT US ▾',
        nav_p_prog: 'PROGRAMS ▾',
        nav_p_edu: 'ACADEMICS ▾',
        nav_p_intl: 'INTERNATIONAL ▾',
        nav_p_contact: 'CONTACT',
        kosen_slogan: 'JAPANESE-STYLE 5-YEAR INTEGRATED ENGINEERING COLLEGE FOR STUDENTS AGED 15 TO 20',
        quota_full: 'FULL',
        quota_seats: 'Seats',
        ticker_badge: 'BRIEF',
        featured_news: 'FEATURED NEWS',
        sec_news: 'NEWS',
        sec_announce: 'ANNOUNCEMENTS',
        footer_contact: 'CONTACT US',
        footer_links: 'QUICK LINKS',
        footer_copy: '© 2026 Darkhan KOSEN College of Technology, MULS. All rights reserved.',
    }
};

function t(key) {
    return (I18N[currentLang] && I18N[currentLang][key]) || key;
}

function toggleLanguage() {
    currentLang = currentLang === 'mn' ? 'en' : 'mn';
    applyTranslations();
    showToast(currentLang === 'mn' ? 'Хэл: Монгол (MN)' : 'Language: English (EN)');

    const pubLang = document.getElementById('pub-lang');
    if (pubLang) {
        pubLang.innerHTML = currentLang === 'mn'
            ? '<u style="color:#ffd700;">MN</u> | EN'
            : 'MN | <u style="color:#ffd700;">EN</u>';
    }

    if (typeof renderPublicHome === 'function') renderPublicHome();
    if (currentUser) renderCurrentLmsView();
}

function applyTranslations() {
    document.querySelectorAll('[data-i18n]').forEach(el => {
        const key = el.getAttribute('data-i18n');
        if (I18N[currentLang][key]) {
            el.innerText = I18N[currentLang][key];
        }
    });

    const langBtn = document.getElementById('lms-lang-btn');
    if (langBtn) langBtn.innerText = currentLang === 'mn' ? '🌐 EN' : '🌐 MN';

    const lblId = document.getElementById('lbl-id');
    if (lblId) lblId.innerText = t('id_' + currentRole);
}