let publicData = {
    admissionOpen: true,
    programs: [],
    news: []
};

function renderPublicHome() {
    const container = document.getElementById('public-home-content');
    if (!container) return;

    const featured = publicData.news.find(n => n.category === 'Тэргүүн мэдээ') || publicData.news[0];
    const sideNews = publicData.news.filter(n => n !== featured).slice(0, 4);

    // Зарлал & Элсэлтийн мэдээнүүдийг баруун баганад, бусад бүх мэдээг зүүн баганад харуулна
    const announcements = publicData.news.filter(n => n.category === 'Зарлал' || n.category === 'Элсэлт');
    const regularNews = publicData.news.filter(n => n.category !== 'Зарлал');

    container.innerHTML = `
        <section class="num-section" style="padding-top: 22px;">
            <div class="ticker-bar" style="cursor:pointer;" onclick="${announcements[0] ? `openArticle(${announcements[0].id})` : ''}">
                <span class="ticker-badge">${t('ticker_badge')}</span>
                <span class="ticker-text">
                    ${announcements[0] ? announcements[0].title : '2026 оны намрын улирлын хичээл сонголт болон элсэлтийн мэдээлэл гарлаа.'}
                </span>
            </div>

            ${featured ? `
            <div class="num-featured-grid">
                <div class="num-main-news" style="background:linear-gradient(180deg, rgba(0,0,0,0.12), rgba(0,0,0,0.86)), ${featured.imageUrl ? `url('${featured.imageUrl}') center/cover` : (featured.imageColor || '#0e4d2d')};"
                     onclick="openArticle(${featured.id})">
                    <span class="news-cat-pill">${featured.category || t('featured_news')}</span>
                    <h2>${featured.title}</h2>
                    <p>${featured.summary}</p>
                    <small>${featured.date}  •  ${featured.views || 1} уншсан</small>
                </div>
                <div class="num-side-grid">
                    ${sideNews.map(n => `
                        <div class="num-side-card" style="background:linear-gradient(180deg, rgba(0,0,0,0.18), rgba(0,0,0,0.88)), ${n.imageUrl ? `url('${n.imageUrl}') center/cover` : (n.imageColor || '#1e293b')};"
                             onclick="openArticle(${n.id})">
                            <span class="side-cat-tag">${n.category || 'Мэдээ'}</span>
                            <h4>${n.title}</h4>
                            <small>${n.date}</small>
                        </div>
                    `).join('')}
                </div>
            </div>` : ''}

            <!-- МЭДЭЭ & ЗАРЛАЛ (2 БАГАНА) -->
            <div class="num-two-col">
                <div>
                    <div class="sec-bar"><span>${t('sec_news')}</span></div>
                    ${regularNews.length === 0 ? '<p style="color:var(--muted);">Мэдээ одоогоор алга.</p>' :
            regularNews.map(n => `
                        <div class="news-list-item" style="cursor:pointer;" onclick="openArticle(${n.id})">
                            <div class="news-thumb" style="background:${n.imageUrl ? `url('${n.imageUrl}') center/cover` : (n.imageColor || '#0e4d2d')};">
                                ${n.imageUrl ? '' : '<span style="font-size:11px; font-weight:700; letter-spacing:0.6px; color:#fff;">KOSEN</span>'}
                            </div>
                            <div>
                                <span class="news-mini-cat">${n.category || 'Мэдээ'}</span>
                                <h4>${n.title}</h4>
                                <p>${n.summary}</p>
                                <small>${n.date}  •  ${n.views || 1} уншсан</small>
                            </div>
                        </div>
                    `).join('')}
                </div>
                <div>
                    <div class="sec-bar"><span>${t('sec_announce')}</span></div>
                    ${announcements.length === 0 ? '<p style="color:var(--muted);">Зарлал одоогоор алга.</p>' :
            announcements.map(a => `
                        <div class="announce-item" style="cursor:pointer;" onclick="openArticle(${a.id})">
                            <div class="announce-tag">${a.category || t('sec_announce')}</div>
                            <div>
                                <h4>${a.title}</h4>
                                <small>${a.date}</small>
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
            <div class="card-title">Элсэлтийн цахим бүртгэл (ЭЕШ-ийн оноо ба Төлбөрийн хөнгөлөлт)</div>
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

            <button class="btn-primary" style="margin-top:14px;" onclick="submitAdmissionForm()">Бүртгэл илгээх</button>
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

// ================= ДЭЭД ЦЭСНИЙ БҮТЭН ХУУДСУУД (БИДНИЙ ТУХАЙ, ХӨТӨЛБӨР, Г.М.) =================
const PUB_INFO_DATA = {
    about: {
        section: 'Бидний тухай',
        title: 'Сургуулийн танилцуулга',
        lead: 'ХААИС-ийн харьяа Дархан Коосэн Технологийн Коллеж нь Япон улсын үндэсний Коосэн (KOSEN) сургалтын тогтолцоог Монгол Улсад хэрэгжүүлэгч инженерийн коллеж юм.',
        body: `
            <p>Дархан Коосэн Технологийн Коллеж нь ерөнхий боловсролын сургуулийн 9-р анги төгсөгчдийг элсүүлэн авч, 5 жилийн нэгдсэн хөтөлбөрөөр онол, практик дадлагыг хослуулан сургаж, олон улсын жишигт нийцсэн инженерийн дэд бакалаврын зэрэгтэй мэргэжилтэн бэлтгэдэг.</p>
            <h3>Сургалтын гол онцлог</h3>
            <ul class="info-list">
                <li><b>5 жилийн тасралтгүй тогтолцоо:</b> Ахлах сургууль болон дээд боловсролын суурь хичээлүүдийг давхардалгүйгээр цогц байдлаар үзнэ.</li>
                <li><b>Лаборатори ба дадлага төвтэй сургалт:</b> 1-р курсээс эхлэн инженерийн лаборатори, төсөлт ажил, үйлдвэрлэлийн дадлагад хамрагдана.</li>
                <li><b>Япон хэл ба соёл:</b> 5 жилийн турш системтэйгээр Япон хэл судалж, JLPT N3–N2 түвшинд хүрэх боломжтой.</li>
            </ul>
            <div class="info-stats-grid">
                <div class="info-stat-box"><b>5 жил</b><span>Нэгдсэн хөтөлбөр</span></div>
                <div class="info-stat-box"><b>4 салбар</b><span>Инженерийн чиглэл</span></div>
                <div class="info-stat-box"><b>70%</b><span>Практик ба лаборатори</span></div>
            </div>
        `
    },
    vision: {
        section: 'Бидний тухай',
        title: 'Алсын хараа, Эрхэм зорилго',
        lead: 'Олон улсад өрсөлдөх чадвартай, бүтээлч сэтгэлгээтэй инженер бэлтгэх нь бидний үндсэн чиг шугам юм.',
        body: `
            <h3>Алсын хараа</h3>
            <p>Монгол Улсын бүс нутгийн хөгжлийг түүчээлэх, Япон загварын инженерийн боловсролоор олон улсад хүлээн зөвшөөрөгдсөн тэргүүлэх технологийн коллеж болох.</p>
            <h3>Эрхэм зорилго</h3>
            <p>Онолын бат бөх мэдлэг, практик ур чадвар, инженерийн ёс зүй болон гадаад хэлний өндөр чадамжтай, хөдөлмөрийн зах зээлд бэлэн мэргэжилтэн бэлтгэх.</p>
            <h3>Үнэт зүйлс</h3>
            <ul class="info-list">
                <li><b>Монозүкүри (Monozukuri):</b> Бүтээн байгуулалт, чанар, нягт нямбай хандлагыг эрхэмлэх.</li>
                <li><b>Бие даасан байдал:</b> Асуудлыг өөрөө олж харах, судалгаа шинжилгээ хийж шийдэл боловсруулах.</li>
                <li><b>Хамтын ажиллагаа:</b> Багаар ажиллах соёл, ёс зүй, хариуцлагыг төлөвшүүлэх.</li>
            </ul>
        `
    },
    history: {
        section: 'Бидний тухай',
        title: 'Түүхэн замнал',
        lead: 'Дархан-Уул аймагт инженерийн боловсролын шинэ жишгийг тогтоон, Япон–Монголын боловсролын хамтын ажиллагааг өргөжүүлж байна.',
        body: `
            <p>Хөдөө Аж Ахуйн Их Сургуулийн бүрэлдэхүүн Дархан Коосэн Технологийн Коллеж нь бүс нутгийн үйлдвэрлэл, дэд бүтэц, мэдээллийн технологийн салбарт чадварлаг инженерүүдийг орон нутагт нь бэлтгэх зорилгоор байгуулагдсан.</p>
            <table class="grades-table" style="margin-top:16px;">
                <thead><tr><th style="width:120px;">Он</th><th>Гол үйл явдал</th></tr></thead>
                <tbody>
                    <tr><td><b>Үүсгэн байгуулалт</b></td><td>Японы Коосэн хөтөлбөрийн жишгээр сургалтын төлөвлөгөө боловсруулж, элсэлт авч эхэлсэн.</td></tr>
                    <tr><td><b>Хамтын ажиллагаа</b></td><td>Японы түнш Коосэн сургуулиуд болон үйлдвэр технологийн компаниудтай санамж бичиг байгуулсан.</td></tr>
                    <tr><td><b>2025–2026 он</b></td><td>Лабораторийн баазыг шинэчилж, оюутан солилцоо болон цахим сургалтын нэгдсэн системийг нэвтрүүлсэн.</td></tr>
                </tbody>
            </table>
        `
    },
    it: {
        section: 'Хөтөлбөр',
        title: 'Мэдээллийн технологи (Information Technology)',
        lead: 'Програм хангамж, систем хөгжүүлэлт, өгөгдлийн сан, сүлжээ болон IoT автоматжуулалтын чиглэлээр мэргэшүүлнэ.',
        applyProgram: 'Мэдээллийн технологи',
        body: `
            <p>Мэдээллийн технологийн хөтөлбөр нь орчин үеийн програм хангамжийн инженерчлэл болон техник хангамжийн интеграцчилалыг хослуулан эзэмшүүлэх зорилготой.</p>
            <h3>Үндсэн мэргэжлийн хичээлүүд</h3>
            <ul class="info-list">
                <li>Визуал програмчлал (C# WinForms / .NET), Java, Вэб хөгжүүлэлт (HTML, CSS, JavaScript)</li>
                <li>Үйлдлийн систем, Компьютерийн сүлжээ, Өгөгдлийн сангийн удирдлагын систем (MySQL)</li>
                <li>Электрон хэлхээ, Дижитал логик, Микроконтроллер ба IoT (Arduino, Raspberry Pi)</li>
                <li>Алгоритм ба өгөгдлийн бүтэц, Инженерийн математик, Япон хэл</li>
            </ul>
            <h3>Төгсөгчийн ажлын байр</h3>
            <p>Програм хангамжийн инженер, Full-stack веб хөгжүүлэгч, Систем ба сүлжээний администратор, Автоматжуулалт ба IoT инженер.</p>
        `
    },
    elec: {
        section: 'Хөтөлбөр',
        title: 'Цахилгаан инженерчлэл (Electrical Engineering)',
        lead: 'Цахилгаан хангамж, электроник, автомат удирдлага болон сэргээгдэх эрчим хүчний мэргэжилтэн бэлтгэнэ.',
        applyProgram: 'Цахилгаан инженерчлэл',
        body: `
            <p>Цахилгаан инженерчлэлийн хөтөлбөр нь үйлдвэр, барилгын цахилгаан хангамж, электрон төхөөрөмжийн зохион бүтээлт, реле хамгаалалт болон автоматжуулалтын гүнзгий мэдлэг олгоно.</p>
            <h3>Үндсэн чиглэлүүд</h3>
            <ul class="info-list">
                <li>Цахилгаан хэлхээний онол ба аналог, тоон электроник</li>
                <li>Цахилгаан машин, трансформатор, эрчим хүчний дамжуулалт</li>
                <li>PLC програмчлал ба үйлдвэрлэлийн автомат удирдлагын систем</li>
                <li>Сэргээгдэх эрчим хүчний эх үүсвэр ба хэмжил зүй</li>
            </ul>
        `
    },
    mech: {
        section: 'Хөтөлбөр',
        title: 'Механик инженерчлэл (Mechanical Engineering)',
        lead: 'Машин үйлдвэрлэл, CAD/CAM зураг төсөл, робот техник болон үйлдвэрийн тоног төхөөрөмжийн инженер бэлтгэнэ.',
        applyProgram: 'Механик инженерчлэл',
        body: `
            <p>Механик инженерчлэлийн хөтөлбөр нь инженерийн зураг зүй, материалын эсэргүүцэл, машин механизмын зохион бүтээлт, CNC болон робот техникийн практик чадварыг олгоно.</p>
            <h3>Үндсэн чиглэлүүд</h3>
            <ul class="info-list">
                <li>Инженерийн 3D загварчлал (CAD/CAM/CAE) ба техникийн зураг зүй</li>
                <li>Материалын эсэргүүцэл, Термодинамик ба Шингэний механик</li>
                <li>Мехатроник, робот техник ба металл боловсруулалтын технологи</li>
            </ul>
        `
    },
    civil: {
        section: 'Хөтөлбөр',
        title: 'Барилгын инженерчлэл (Civil Engineering)',
        lead: 'Барилгын бүтээц, зураг төсөл, геодези, дэд бүтэц болон ногоон барилгын технологийн инженер бэлтгэнэ.',
        applyProgram: 'Барилгын инженерчлэл',
        body: `
            <p>Барилгын инженерчлэлийн хөтөлбөр нь иргэний болон үйлдвэрийн барилга байгууламжийн тооцоо, зураг төсөл, барилгын материалын шинжилгээ, талбайн удирдлагын цогц мэдлэг олгоно.</p>
            <h3>Үндсэн чиглэлүүд</h3>
            <ul class="info-list">
                <li>Барилгын механик, Төмөр бетон ба металл бүтээц</li>
                <li>Геодези, Хөрсний механик ба суурийн тооцоо</li>
                <li>Барилгын зураг төсөл (AutoCAD, BIM) ба төслийн менежмент</li>
            </ul>
        `
    },
    schedule: {
        section: 'Сургалтын үйл ажиллагаа',
        title: 'Сургалтын нэгдсэн хуваарь',
        lead: '2026–2027 оны хичээлийн жилийн улирал, шалгалт болон үйлдвэрлэлийн дадлагын нэгдсэн календарь.',
        body: `
            <table class="grades-table">
                <thead><tr><th>Үйл ажиллагаа</th><th>Хугацаа</th><th>Тайлбар</th></tr></thead>
                <tbody>
                    <tr><td><b>Намрын улирал</b></td><td>9-р сарын 1 – 12-р сарын 25</td><td>16 долоо хоногийн танхимын сургалт</td></tr>
                    <tr><td><b>Намрын шалгалтын 7 хоног</b></td><td>12-р сарын 15 – 12-р сарын 25</td><td>Улирлын эцсийн шалгалт, тайлан хамгаалалт</td></tr>
                    <tr><td><b>Өвлийн амралт</b></td><td>12-р сарын 26 – 1-р сарын 19</td><td>Оюутны амралт</td></tr>
                    <tr><td><b>Хаврын улирал</b></td><td>1-р сарын 20 – 5-р сарын 30</td><td>16 долоо хоногийн танхимын сургалт</td></tr>
                    <tr><td><b>Үйлдвэрлэлийн дадлага</b></td><td>6-р сарын 1 – 6-р сарын 25</td><td>3–5-р курсийн оюутнууд үйлдвэр, байгууллагад дадлага хийнэ</td></tr>
                </tbody>
            </table>
        `
    },
    scholarship: {
        section: 'Сургалтын үйл ажиллагаа',
        title: 'Тэтгэлэг ба төлбөрийн хөнгөлөлт',
        lead: 'Сурлагын амжилт болон ЭЕШ-ийн онооноос хамааран 25%–100% хүртэлх сургалтын төлбөрийн хөнгөлөлт, гадаад тэтгэлэгт хамрагдах боломжтой.',
        body: `
            <h3>Элсэгчдэд олгох ЭЕШ-ийн тэтгэлэг</h3>
            <table class="grades-table">
                <thead><tr><th>Болзол (ЭЕШ-ийн оноо)</th><th>Хөнгөлөлтийн хувь</th><th>Хамрах хугацаа</th></tr></thead>
                <tbody>
                    <tr><td>750 болон түүнээс дээш</td><td><b>100% чөлөөлнө</b></td><td>Эхний хичээлийн жил</td></tr>
                    <tr><td>650 – 749 оноо</td><td><b>50% хөнгөлнө</b></td><td>Эхний хичээлийн жил</td></tr>
                    <tr><td>550 – 649 оноо</td><td><b>25% хөнгөлнө</b></td><td>Эхний хичээлийн жил</td></tr>
                </tbody>
            </table>
            <h3 style="margin-top:20px;">Суралцагчдын урамшуулалт тэтгэлэг</h3>
            <ul class="info-list">
                <li>Голч дүн 3.6-аас дээш суралцагчид Япон улсын засгийн газар (MEXT) болон хамтрагч байгууллагуудын тэтгэлэгт нэр дэвших эрхтэй.</li>
                <li>Улсын болон олон улсын олимпиад, Робокон тэмцээнд шагналт байр эзэлсэн оюутнуудад сургуулийн нэрэмжит тэтгэлэг олгоно.</li>
            </ul>
        `
    },
    dorm: {
        section: 'Сургалтын үйл ажиллагаа',
        title: 'Оюутны дотуур байр',
        lead: 'Хөдөө орон нутгаас ирж суралцаж буй оюутнуудад зориулсан аюулгүй, тохилог амьдрах орчин.',
        body: `
            <p>ХААИС-ийн Дархан дахь оюутны дотуур байр нь хичээлийн байртай ойр байрладаг бөгөөд оюутнуудын сурч боловсрох, амрах нөхцөлийг бүрэн хангасан.</p>
            <h3>Нөхцөл ба боломжууд</h3>
            <ul class="info-list">
                <li>Өндөр хурдны утасгүй интернэт (Wi-Fi) болон хичээл давтах уншлагын өрөө</li>
                <li>24 цагийн харуул хамгаалалт, камерын хяналт, жижүүр багшийн хяналт</li>
                <li>Халуун хүйтэн ус, гал тогоо, хувцас угаалгын хэсэг</li>
                <li>Спорт заал болон чөлөөт цагийн танхим ашиглах боломж</li>
            </ul>
        `
    },
    japan: {
        section: 'Гадаад харилцаа',
        title: 'Япон улсын Коосэн хамтын ажиллагаа',
        lead: 'Японы үндэсний Коосэн (NIT) болон технологийн их сургуулиудтай нягт хамтран ажиллаж, төгсөгчдөд дэвшин суралцах болон ажиллах гүүр болдог.',
        body: `
            <p>Дархан Коосэн нь Японы инженерийн боловсролын стандартыг нэвтрүүлснээр төгсөгчид Япон улсын их сургуулиудын 3-р курст шууд шилжин суралцах, мөн Японы технологи, электроник, IT компаниудад үндсэн инженерээр ажиллах өргөн боломжтой.</p>
            <h3>Хамтын ажиллагааны чиглэлүүд</h3>
            <ul class="info-list">
                <li>Япон багш, мэргэжилтнүүдийн зочин лекц болон хамтарсан сургалт</li>
                <li>Токио болон бусад хотуудын технологийн компаниудтай ажлын ярилцлага, танилцах дадлага</li>
                <li>Японы их сургуулиудад (3-р курс рүү) элсэх бэлтгэл болон зөвлөгөө</li>
            </ul>
        `
    },
    exchange: {
        section: 'Гадаад харилцаа',
        title: 'Оюутан солилцооны хөтөлбөр',
        lead: 'Шилдэг оюутнууд жил бүр Токиогийн түнш Коосэн сургуульд богино болон урт хугацааны солилцооны хөтөлбөрт хамрагддаг.',
        body: `
            <p>Оюутан солилцооны хөтөлбөрийн хүрээнд оюутнууд Япон улсын Коосэн сургуулийн хичээл, лабораторийн орчинд суралцаж, технологийн музей, үйлдвэрүүдтэй танилцан, Япон оюутнуудтай хамтарсан төсөл хэрэгжүүлдэг.</p>
            <h3>Шалгуур үзүүлэлт</h3>
            <ul class="info-list">
                <li>Сурлагын голч дүн 3.5 болон түүнээс дээш байх</li>
                <li>Япон хэлний JLPT N5–N3 түвшний мэдлэгтэй байх</li>
                <li>Сургуулийн олон нийтийн ажил, клуб, тэмцээнд идэвхтэй оролцдог байх</li>
            </ul>
        `
    },
    contact: {
        section: 'Холбоо барих',
        title: 'Холбоо барих мэдээлэл',
        lead: 'Элсэлт, сургалт болон хамтын ажиллагаатай холбоотой бүх асуудлаар доорх хаягаар холбогдоно уу.',
        body: `
            <table class="grades-table">
                <tbody>
                    <tr><td style="width:180px; color:var(--muted);">Байршил / Хаяг</td><td><b>Дархан-Уул аймаг, Дархан сум, ХААИС-ийн хичээлийн байр</b></td></tr>
                    <tr><td style="color:var(--muted);">Утасны дугаар</td><td><b>7037-3737</b></td></tr>
                    <tr><td style="color:var(--muted);">Албан ёсны имэйл</td><td><b>info@kosen.edu.mn</b></td></tr>
                    <tr><td style="color:var(--muted);">Ажлын цаг</td><td>Даваа – Баасан, 08:30 – 17:30 (Цайны цаг 12:30 – 13:30)</td></tr>
                </tbody>
            </table>
            <div style="margin-top:20px; padding:16px 18px; background:var(--surface-2, #f8fafc); border:1px solid var(--border, #e2e8f0); border-radius:10px;">
                <b style="display:block; margin-bottom:6px;">Элсэлтийн комисс</b>
                <p style="margin:0; font-size:14px; color:var(--muted);">Цахим элсэлтийн бүртгэл болон ЭЕШ-ийн тэтгэлгийн тооцоолуурыг «Элсэлт» цэсээр орж шууд ашиглах боломжтой.</p>
                <button class="btn-primary" style="margin-top:12px;" onclick="openAdmissions()">Элсэлтийн хуудас руу очих</button>
            </div>
        `
    }
};

function openPubInfo(key) {
    const info = PUB_INFO_DATA[key];
    if (!info) return;

    closePubInfo();
    showView('view-article');
    renderPubInfoPage(key, info);
    window.scrollTo(0, 0);
}

function renderPubInfoPage(activeKey, info) {
    const body = document.getElementById('article-detail-body');
    if (!body) return;

    // Тухайн бүлгийн болон бусад үндсэн хуудсуудыг баруун цэсэнд харуулах
    const sameSectionKeys = Object.keys(PUB_INFO_DATA).filter(k => PUB_INFO_DATA[k].section === info.section);
    const otherSectionKeys = Object.keys(PUB_INFO_DATA).filter(k => PUB_INFO_DATA[k].section !== info.section).slice(0, 6);

    body.innerHTML = `
        <div class="article-layout">
            <!-- ЗҮҮН БАГАНА: БҮТЭН МЭДЭЭЛЛИЙН ХУУДАС -->
            <div class="article-main">
                <div style="display:flex; justify-content:space-between; align-items:center; flex-wrap:wrap; gap:10px; margin-bottom:14px;">
                    <span class="news-cat-pill" style="background:var(--green-dark); color:#ffffff;">${info.section}</span>
                    <div style="display:flex; gap:8px;">
                        <button class="m-select" style="cursor:pointer; font-size:12.5px;" onclick="printPubInfo('${activeKey}')">Хэвлэх</button>
                        <button class="m-select" style="cursor:pointer; font-size:12.5px;" onclick="showView('view-public')">Нүүр хуудас</button>
                    </div>
                </div>

                <h1 class="article-title">${info.title}</h1>

                <div class="article-lead">${info.lead}</div>

                <div class="article-content info-page-content">
                    ${info.body}
                </div>

                ${info.applyProgram ? `
                    <div style="margin-top:26px; padding-top:18px; border-top:1px solid var(--border, #e2e8f0); display:flex; justify-content:space-between; align-items:center; flex-wrap:wrap; gap:12px;">
                        <span style="font-size:14px; color:var(--muted);">Энэ хөтөлбөрт элсэх сонирхолтой бол цахим бүртгэлээ шууд илгээгээрэй.</span>
                        <button class="btn-primary" onclick="openAdmissions('${info.applyProgram}')">Энэ хөтөлбөрт бүртгүүлэх</button>
                    </div>
                ` : ''}
            </div>

            <!-- БАРУУН БАГАНА: БҮЛГИЙН ЦЭС -->
            <div class="article-sidebar">
                <div class="sec-bar"><span>${info.section.toUpperCase()}</span></div>
                <div class="info-side-nav">
                    ${sameSectionKeys.map(k => `
                        <button class="info-side-btn ${k === activeKey ? 'active' : ''}" onclick="openPubInfo('${k}')">
                            ${PUB_INFO_DATA[k].title}
                        </button>
                    `).join('')}
                </div>

                <div class="sec-bar" style="margin-top:22px;"><span>БУСАД МЭДЭЭЛЭЛ</span></div>
                <div class="info-side-nav">
                    ${otherSectionKeys.map(k => `
                        <button class="info-side-btn" onclick="openPubInfo('${k}')">
                            ${PUB_INFO_DATA[k].title}
                        </button>
                    `).join('')}
                </div>
            </div>
        </div>
    `;
}

function printPubInfo(key) {
    const info = PUB_INFO_DATA[key];
    if (!info) return;
    printDocument(info.title, `
        <p style="color:#475569; font-size:13px;"><b>Бүлэг:</b> ${info.section}</p>
        <p style="font-weight:600; margin:12px 0;">${info.lead}</p>
        <div style="font-size:14px; line-height:1.8;">${info.body}</div>
    `);
}

function closePubInfo() {
    document.getElementById('pub-info-modal')?.classList.remove('open');
}

// ================= МЭДЭЭНИЙ ДЭЛГЭРЭНГҮЙ (УРТ МЭДЭЭ УНШИХ ЦОНХ) =================
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

    const mostRead = [...publicData.news]
        .sort((a, b) => (b.views || 0) - (a.views || 0))
        .slice(0, 5);

    const rawText = (article.content || article.summary || '').replace(/\\n/g, '\n');
    const paragraphs = rawText.split('\n').map(p => p.trim()).filter(p => p !== '');
    const wordCount = rawText.split(/\s+/).filter(Boolean).length;
    const readMins = Math.max(1, Math.ceil(wordCount / 160));

    const imgHtml = article.imageUrl
        ? `<img src="${article.imageUrl}" class="article-img-${article.imagePosition || 'top'}" alt="Мэдээний зураг">`
        : `<div class="article-cover-placeholder" style="background:linear-gradient(135deg, ${article.imageColor || '#0b3b24'}, #0f172a);">
               <div class="logo-circle" style="width:58px; height:58px;">ХААИС</div>
               <div>
                   <div style="font-weight:800; font-size:15px;">ДАРХАН КООСЭН МЭДЭЭ</div>
                   <div style="font-size:12px; opacity:0.8;">${article.category || 'Албан ёсны мэдээлэл'} • ${article.date}</div>
               </div>
           </div>`;

    let contentHtml = '';
    if (article.imageUrl && article.imagePosition === 'middle' && paragraphs.length > 1) {
        contentHtml = `<p>${paragraphs[0]}</p>${imgHtml}${paragraphs.slice(1).map(p => `<p>${p}</p>`).join('')}`;
    } else if (article.imageUrl && article.imagePosition === 'left') {
        contentHtml = `<div class="article-float-wrap">${imgHtml}${paragraphs.map(p => `<p>${p}</p>`).join('')}</div>`;
    } else {
        contentHtml = `${imgHtml}${paragraphs.map(p => `<p>${p}</p>`).join('')}`;
    }

    body.innerHTML = `
        <div class="article-layout">
            <!-- ЗҮҮН БАГАНА: ҮНДСЭН МЭДЭЭ -->
            <div class="article-main">
                <div style="display:flex; justify-content:space-between; align-items:center; flex-wrap:wrap; gap:10px; margin-bottom:12px;">
                    <span class="news-cat-pill" style="background:var(--green-dark); color:white;">${article.category || 'Мэдээ'}</span>
                    <button class="m-select" style="cursor:pointer; font-size:12.5px;" onclick="printArticle(${article.id})">Хэвлэх</button>
                </div>

                <h1 class="article-title">${article.title}</h1>

                <div class="article-meta">
                    <span>Огноо: ${article.date}</span>
                    <span>Үзсэн: ${article.views || 1}</span>
                    <span>Унших хугацаа: ~${readMins} мин</span>
                </div>

                ${article.summary ? `<div class="article-lead">${article.summary}</div>` : ''}

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
                            ${m.imageUrl ? '' : '<span style="font-size:10px; font-weight:700; color:#fff;">KOSEN</span>'}
                        </div>
                        <div>
                            <h4>${m.title}</h4>
                            <small>${m.date}  •  ${m.views || 1} уншсан</small>
                        </div>
                    </div>
                `).join('')}
            </div>
        </div>
    `;
}

function printArticle(id) {
    const article = publicData.news.find(n => n.id === id);
    if (!article) return;
    const paragraphs = (article.content || article.summary || '').replace(/\\n/g, '\n').split('\n').filter(p => p.trim() !== '');
    printDocument(article.title, `
        <p style="color:#475569; font-size:13px;"><b>Ангилал:</b> ${article.category}  |  <b>Огноо:</b> ${article.date}</p>
        ${article.imageUrl ? `<img src="${article.imageUrl}" style="max-width:100%; max-height:300px; object-fit:cover; border-radius:8px; margin:12px 0;">` : ''}
        <div style="font-size:14px; line-height:1.8; margin-top:12px;">
            ${paragraphs.map(p => `<p style="margin-bottom:12px;">${p}</p>`).join('')}
        </div>
    `);
}