let publicData = {
    admissionOpen: true,
    programs: [],
    news: []
};

function renderPublicHome() {
    const container = document.getElementById('public-home-content');
    if (!container) return;

    const progs = publicData.programs;
    const leftProgs = progs.slice(0, 2);
    const rightProgs = progs.slice(2, 4);

    const featured = publicData.news.find(n => n.category === 'Тэргүүн мэдээ') || publicData.news[0];
    const sideNews = publicData.news.filter(n => n !== featured).slice(0, 4);
    const regularNews = publicData.news.filter(n => n.category === 'Мэдээ');
    const announcements = publicData.news.filter(n => n.category === 'Зарлал');

    const majorColors = ['#1e3a8a', '#0f766e', '#b45309', '#4338ca'];

    container.innerHTML = `
        <!-- 1. ШУТИС КООСЭН ЗАГВАРЫН 4 МЭРГЭЖЛИЙН ХЭСЭГ -->
        <section class="kosen-hero-section">
            <div class="kosen-hero-grid">
                ${leftProgs.map((p, i) => `
                    <div class="major-card" onclick="openAdmissions('${p.name}')">
                        <div class="major-top-tag">${p.engName || 'ENGINEER'}</div>
                        <div class="major-visual" style="background:linear-gradient(135deg, ${majorColors[i]}, #0f172a);">
                            <div class="major-quota-pill ${p.enrolled >= p.quota ? 'full' : 'open'}">
                                ${p.enrolled >= p.quota ? t('quota_full') : `${t('quota_seats')}: ${p.enrolled}/${p.quota}`}
                            </div>
                            <span style="font-size:44px;">${i === 0 ? '💻' : '⚡'}</span>
                        </div>
                        <div class="major-bottom-bar">${p.name}</div>
                    </div>
                `).join('')}

                <div class="major-center-box">
                    <div class="logo-circle" style="width:92px; height:92px; font-size:13px; border-width:3px; margin:0 auto 14px;">
                        ХААИС<br>ДАРХАН<br>КООСЭН
                    </div>
                    <h3>${t('kosen_slogan')}</h3>
                </div>

                ${rightProgs.map((p, i) => `
                    <div class="major-card" onclick="openAdmissions('${p.name}')">
                        <div class="major-top-tag">${p.engName || 'ENGINEER'}</div>
                        <div class="major-visual" style="background:linear-gradient(135deg, ${majorColors[i + 2]}, #0f172a);">
                            <div class="major-quota-pill ${p.enrolled >= p.quota ? 'full' : 'open'}">
                                ${p.enrolled >= p.quota ? t('quota_full') : `${t('quota_seats')}: ${p.enrolled}/${p.quota}`}
                            </div>
                            <span style="font-size:44px;">${i === 0 ? '⚙️' : '🏗️'}</span>
                        </div>
                        <div class="major-bottom-bar">${p.name}</div>
                    </div>
                `).join('')}
            </div>
        </section>

        <!-- 2. МУИС ЗАГВАРЫН "ЦӨӨН МӨРӨӨР" & ТЭРГҮҮН МЭДЭЭ -->
        <section class="num-section">
            <div class="ticker-bar">
                <span class="ticker-badge">${t('ticker_badge')}</span>
                <span class="ticker-text">
                    ${announcements[0] ? announcements[0].title : '2026 оны намрын улирлын хичээл сонголт болон элсэлтийн мэдээлэл гарлаа.'}
                </span>
            </div>

            ${featured ? `
            <div class="num-featured-grid">
                <div class="num-main-news" style="background:linear-gradient(180deg, rgba(0,0,0,0.15), rgba(0,0,0,0.85)), ${featured.imageUrl ? `url('${featured.imageUrl}') center/cover` : (featured.imageColor || '#0e4d2d')};"
                     onclick="openArticle(${featured.id})">
                    <span class="news-cat-pill">${t('featured_news')}</span>
                    <h2>${featured.title}</h2>
                    <p>${featured.summary}</p>
                    <small>🕒 ${featured.date}</small>
                </div>
                <div class="num-side-grid">
                    ${sideNews.map(n => `
                        <div class="num-side-card" style="background:linear-gradient(180deg, rgba(0,0,0,0.2), rgba(0,0,0,0.88)), ${n.imageUrl ? `url('${n.imageUrl}') center/cover` : (n.imageColor || '#1e293b')};"
                             onclick="openArticle(${n.id})">
                            <h4>${n.title}</h4>
                            <small>🕒 ${n.date}</small>
                        </div>
                    `).join('')}
                </div>
            </div>` : ''}

            <!-- 3. МЭДЭЭ & ЗАРЛАЛ (2 БАГАНА) -->
            <div class="num-two-col">
                <div>
                    <div class="sec-bar"><span>${t('sec_news')}</span></div>
                    ${regularNews.length === 0 ? '<p style="color:var(--muted);">Мэдээ одоогоор алга.</p>' :
            regularNews.map(n => `
                        <div class="news-list-item" style="cursor:pointer;" onclick="openArticle(${n.id})">
                            <div class="news-thumb" style="background:${n.imageUrl ? `url('${n.imageUrl}') center/cover` : (n.imageColor || '#0e4d2d')};">
                                ${n.imageUrl ? '' : '📰'}
                            </div>
                            <div>
                                <h4>${n.title}</h4>
                                <p>${n.summary}</p>
                                <small>🕒 ${n.date}</small>
                            </div>
                        </div>
                    `).join('')}
                </div>
                <div>
                    <div class="sec-bar"><span>${t('sec_announce')}</span></div>
                    ${announcements.length === 0 ? '<p style="color:var(--muted);">Зарлал одоогоор алга.</p>' :
            announcements.map(a => `
                        <div class="announce-item" style="cursor:pointer;" onclick="openArticle(${a.id})">
                            <div class="announce-tag">${t('sec_announce')}</div>
                            <div>
                                <h4>${a.title}</h4>
                                <small>🕒 ${a.date}</small>
                            </div>
                        </div>
                    `).join('')}
                </div>
            </div>
        </section>
    `;
}

// ================= ЭЛСЭЛТИЙН СИСТЕМ =================
function openAdmissions(preselectProgram = '') {
    if (!publicData.admissionOpen) {
        if (confirm('Элсэлтийн бүртгэл дууссан байна!\n\nБүртгэлтэй элсэгч эсвэл оюутан бол систем рүү нэвтэрч орно уу. Нэвтрэх хуудас руу шилжих үү?')) {
            showView('view-login');
        }
        return;
    }

    showView('view-admissions');
    renderAdmissionsPage(preselectProgram);
}

function renderAdmissionsPage(preselectProgram = '') {
    const body = document.getElementById('admissions-body');
    if (!body) return;

    body.innerHTML = `
        <div class="adm-prog-grid">
            ${publicData.programs.map(p => {
        const isFull = p.enrolled >= p.quota;
        const left = Math.max(0, p.quota - p.enrolled);
        return `
                <div class="adm-prog-card ${isFull ? 'is-full' : ''}">
                    <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:8px;">
                        <b style="color:var(--green-dark); font-size:15px;">${p.name}</b>
                        <span class="adm-badge ${isFull ? 'full' : 'open'}">${isFull ? 'ДҮҮРСЭН' : `${left} сул суудал`}</span>
                    </div>
                    <div style="font-size:13px; color:#475569;">Хяналтын тоо: <b>${p.enrolled} / ${p.quota}</b></div>
                    <div style="font-size:13px; color:#475569; margin-top:4px;">Жилийн төлбөр: <b>${p.tuition.toLocaleString()}₮</b></div>
                </div>`;
    }).join('')}
        </div>

        <div class="moodle-card" style="margin-top:22px;">
            <div class="card-title">📝 Элсэлтийн цахим бүртгэл (ЭЕШ-ийн оноо & Төлбөрийн хөнгөлөлт)</div>
            <div class="input-row">
                <div class="input-group">
                    <label>Овог, Нэр:</label>
                    <input type="text" id="adm-name" placeholder="Жнь: Б. Тэмүүлэн">
                </div>
                <div class="input-group">
                    <label>Регистрийн дугаар:</label>
                    <input type="text" id="adm-reg" placeholder="Жнь: УБ08070612">
                </div>
                <div class="input-group">
                    <label>Утасны дугаар:</label>
                    <input type="text" id="adm-phone" placeholder="99112233">
                </div>
            </div>
            <div class="input-row">
                <div class="input-group" style="flex:2;">
                    <label>Элсэх хөтөлбөр / Мэргэжил:</label>
                    <select id="adm-program" onchange="calcAdmissionDiscount()">
                        ${publicData.programs.map(p => {
        const isFull = p.enrolled >= p.quota;
        return `<option value="${p.name}" data-fee="${p.tuition}" ${isFull ? 'disabled' : ''} ${p.name === preselectProgram && !isFull ? 'selected' : ''}>
                                ${p.name} ${isFull ? '(ДҮҮРСЭН)' : `(${p.tuition.toLocaleString()}₮)`}
                            </option>`;
    }).join('')}
                    </select>
                </div>
                <div class="input-group">
                    <label>ЭЕШ-ийн оноо (400-800):</label>
                    <input type="number" id="adm-eyesh" min="400" max="800" value="650" oninput="calcAdmissionDiscount()">
                </div>
            </div>

            <div class="discount-preview" id="adm-discount-box"></div>

            <button class="btn-primary" style="margin-top:14px;" onclick="submitAdmissionForm()">✓ Бүртгэл илгээх</button>
        </div>
    `;

    calcAdmissionDiscount();
}

function calcAdmissionDiscount() {
    const sel = document.getElementById('adm-program');
    const score = parseInt(document.getElementById('adm-eyesh')?.value) || 0;
    const box = document.getElementById('adm-discount-box');
    if (!sel || !box) return;

    const fee = parseInt(sel.options[sel.selectedIndex]?.getAttribute('data-fee')) || 3200000;
    let discount = 0;
    if (score >= 750) discount = 100;
    else if (score >= 650) discount = 50;
    else if (score >= 550) discount = 25;

    const finalFee = Math.round(fee * (100 - discount) / 100);

    box.innerHTML = `
        <div><b>Үндсэн төлбөр:</b> ${fee.toLocaleString()}₮</div>
        <div><b>ЭЕШ-ийн хөнгөлөлт:</b> <span style="color:#16a34a; font-weight:700;">${discount}%</span> (750+ ➔ 100%, 650+ ➔ 50%, 550+ ➔ 25%)</div>
        <div style="font-size:16px; margin-top:4px;"><b>Төлөх дүн:</b> <span style="color:var(--green-dark); font-weight:800;">${finalFee.toLocaleString()}₮</span></div>
    `;
}

function submitAdmissionForm() {
    const fullName = document.getElementById('adm-name').value.trim();
    const registerNo = document.getElementById('adm-reg').value.trim();
    const phone = document.getElementById('adm-phone').value.trim();
    const sel = document.getElementById('adm-program');
    const programName = sel.value;
    const eyeshScore = parseInt(document.getElementById('adm-eyesh').value) || 0;

    if (!fullName || !registerNo || !phone || !programName) {
        return showToast('Бүх талбарыг бүрэн бөглөнө үү.');
    }
    if (eyeshScore < 400 || eyeshScore > 800) {
        return showToast('ЭЕШ-ийн оноо 400-800 хооронд байх ёстой.');
    }

    const fee = parseInt(sel.options[sel.selectedIndex]?.getAttribute('data-fee')) || 3200000;
    let discount = eyeshScore >= 750 ? 100 : eyeshScore >= 650 ? 50 : eyeshScore >= 550 ? 25 : 0;
    const finalTuition = Math.round(fee * (100 - discount) / 100);

    postToCSharp({
        action: 'submitAdmission',
        fullName, registerNo, phone, programName, eyeshScore, discount, finalTuition
    });
}

// ================= ДЭЭД ЦЭСНИЙ МЭДЭЭЛЛИЙН ЦОНХ =================
const PUB_INFO_DATA = {
    about: { title: 'Сургуулийн танилцуулга', body: 'ХААИС-ийн харьяа Дархан Коосэн Технологийн Коллеж нь Япон улсын үндэсний Коосэн (KOSEN) сургалтын тогтолцоог Монгол Улсад нутагшуулж, ЕБС-ийн 9-р анги төгсөгчдийг 5 жилийн хугацаанд онол, дадлага хосолсон чадварлаг инженер болгон бэлтгэдэг.' },
    vision: { title: 'Алсын хараа, Эрхэм зорилго', body: '<b>Алсын хараа:</b> Олон улсад өрсөлдөх чадвартай, бүтээлч инженерүүдийг бэлтгэх бүсийн тэргүүлэх технологийн коллеж болох.<br><br><b>Эрхэм зорилго:</b> Практик ур чадвар, инженерийн ёс зүй, япон хэлний мэдлэгтэй мэргэжилтэн бэлтгэх.' },
    history: { title: 'Түүхэн замнал', body: 'Дархан-Уул аймагт инженерийн боловсролыг хөгжүүлэх зорилгоор байгуулагдаж, Япон улсын Токиогийн Коосэн болон үйлдвэр аж ахуйн байгууллагуудтай нягт хамтран ажиллаж байна.' },
    it: { title: 'Мэдээллийн технологи (IT Engineer)', body: 'Програм хангамж, веб болон гар утасны хөгжүүлэлт, хиймэл оюун ухаан, өгөгдлийн сан, микроконтроллер (Arduino, Raspberry Pi) болон автоматжуулалтын чиглэлээр мэргэшүүлнэ.' },
    elec: { title: 'Цахилгаан инженерчлэл', body: 'Цахилгаан хангамж, электрон хэлхээ, дижитал логик, сэргээгдэх эрчим хүч болон үйлдвэрийн автоматжуулалтын инженер бэлтгэнэ.' },
    mech: { title: 'Механик инженерчлэл', body: 'Машин үйлдвэрлэл, CAD/CAM зураг төсөл, робот техник, металл боловсруулалт болон үйлдвэрлэлийн тоног төхөөрөмжийн инженер бэлтгэнэ.' },
    civil: { title: 'Барилгын инженерчлэл', body: 'Барилгын бүтээц, зураг төсөл, геодези, дэд бүтэц болон орчин үеийн ногоон барилгын технологийн инженер бэлтгэнэ.' },
    schedule: { title: 'Сургалтын нэгдсэн хуваарь', body: 'Намрын улирал: 9-р сарын 1 – 12-р сарын 25.<br>Хаврын улирал: 1-р сарын 20 – 5-р сарын 30.<br>Үйлдвэрлэлийн дадлага: 6-р сард зохион байгуулагдана.' },
    scholarship: { title: 'Тэтгэлэг ба төлбөрийн хөнгөлөлт', body: '• ЭЕШ 750+ оноотой элсэгч: <b>100% хөнгөлөлт</b><br>• ЭЕШ 650+ оноотой элсэгч: <b>50% хөнгөлөлт</b><br>• ЭЕШ 550+ оноотой элсэгч: <b>25% хөнгөлөлт</b><br>• Мөн голч оноо 3.6+ оюутнууд Япон улсын тэтгэлэгт хамрагдах боломжтой.' },
    dorm: { title: 'Оюутны дотуур байр', body: 'Хөдөө орон нутгаас ирж суралцаж буй оюутнуудад зориулсан дулаан, тохилог, өндөр хурдны интернет бүхий оюутны дотуур байртай.' },
    japan: { title: 'Япон улсын Коосэн хамтын ажиллагаа', body: 'Төгсөгчид Япон улсын их сургуулиудын 3-р курст шууд дэвшин суралцах болон Японы технологийн компаниудад инженерээр ажиллах өргөн боломжтой.' },
    exchange: { title: 'Оюутан солилцооны хөтөлбөр', body: 'Жил бүр шилдэг оюутнууд Токиогийн Шинагава Коосэн болон хамтрагч сургуулиудад богино болон урт хугацааны солилцооны хөтөлбөрт хамрагддаг.' },
    contact: { title: 'Холбоо барих', body: '<b>Хаяг:</b> Дархан-Уул аймаг, Дархан сум, ХААИС-ийн хичээлийн байр<br><b>Утас:</b> 7037-3737<br><b>Имэйл:</b> info@kosen.edu.mn<br><b>Ажлын цаг:</b> Даваа–Баасан, 08:30 – 17:30' }
};

function openPubInfo(key) {
    const info = PUB_INFO_DATA[key];
    if (!info) return;
    document.getElementById('pub-info-title').innerText = info.title;
    document.getElementById('pub-info-body').innerHTML = info.body;
    document.getElementById('pub-info-modal').classList.add('open');
}

function closePubInfo() {
    document.getElementById('pub-info-modal').classList.remove('open');
}

// ================= МЭДЭЭНИЙ ДЭЛГЭРЭНГҮЙ (МУИС ЗАГВАР) =================
function openArticle(id) {
    const article = publicData.news.find(n => n.id === id);
    if (!article) return;

    // Үзэлтийн тоог MySQL дээр +1 нэмэх
    article.views = (article.views || 0) + 1;
    postToCSharp({ action: 'incrementNewsView', id });

    showView('view-article');
    renderArticleDetail(article);
    window.scrollTo(0, 0);
}

function renderArticleDetail(article) {
    const body = document.getElementById('article-detail-body');
    if (!body) return;

    // Их уншсан 5 мэдээ (үзэлтийн тоогоор буурах)
    const mostRead = [...publicData.news]
        .sort((a, b) => (b.views || 0) - (a.views || 0))
        .slice(0, 5);

    const paragraphs = (article.content || article.summary || '').split('\n').filter(p => p.trim() !== '');
    const imgHtml = article.imageUrl
        ? `<img src="${article.imageUrl}" class="article-img-${article.imagePosition || 'top'}" alt="Мэдээний зураг">`
        : '';

    let contentHtml = '';
    if (article.imagePosition === 'middle' && paragraphs.length > 1) {
        contentHtml = `<p>${paragraphs[0]}</p>${imgHtml}${paragraphs.slice(1).map(p => `<p>${p}</p>`).join('')}`;
    } else if (article.imagePosition === 'left') {
        contentHtml = `<div class="article-float-wrap">${imgHtml}${paragraphs.map(p => `<p>${p}</p>`).join('')}</div>`;
    } else {
        // default: top
        contentHtml = `${imgHtml}${paragraphs.map(p => `<p>${p}</p>`).join('')}`;
    }

    body.innerHTML = `
        <div class="article-layout">
            <!-- ЗҮҮН БАГАНА: ҮНДСЭН МЭДЭЭ -->
            <div class="article-main">
                <span class="news-cat-pill" style="background:var(--green-dark); color:white;">${article.category}</span>
                <h1 class="article-title">${article.title}</h1>
                <div class="article-meta">
                    <span>🕒 ${article.date}</span>
                    <span>👁️ ${article.views || 1} уншсан</span>
                </div>
                <div class="article-content">
                    ${contentHtml}
                </div>
            </div>

            <!-- БАРУУН БАГАНА: ИХ УНШСАН -->
            <div class="article-sidebar">
                <div class="sec-bar"><span>ИХ УНШСАН</span></div>
                ${mostRead.map(m => `
                    <div class="most-read-item" onclick="openArticle(${m.id})">
                        <div class="most-read-thumb" style="background:${m.imageUrl ? `url('${m.imageUrl}') center/cover` : (m.imageColor || '#0e4d2d')};">
                            ${m.imageUrl ? '' : '📰'}
                        </div>
                        <div>
                            <h4>${m.title}</h4>
                            <small>🕒 ${m.date}   •   👁️ ${m.views || 1}</small>
                        </div>
                    </div>
                `).join('')}
            </div>
        </div>
    `;
}