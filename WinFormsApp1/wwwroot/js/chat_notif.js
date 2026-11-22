// ================= NOTIFICATIONS =================
function updateNotifBadge() {
    const unreadCount = notificationsData.filter(n => !n.isRead).length;
    const badge = document.getElementById('notif-count');
    if (!badge) return;

    if (unreadCount > 0) {
        badge.innerText = unreadCount;
        badge.style.display = 'inline-block';
    } else {
        badge.style.display = 'none';
    }
}

function renderNotifications() {
    const listEl = document.getElementById('notif-list');
    if (!listEl) return;

    if (notificationsData.length === 0) {
        listEl.innerHTML = `<div style="padding:24px; text-align:center; color:var(--muted); font-size:13px;">Мэдэгдэл байхгүй байна.</div>`;
        return;
    }

    listEl.innerHTML = notificationsData.map(n => `
        <div class="notif-item ${n.isRead ? '' : 'unread'}" onclick="readSingleNotif(${n.id}, '${n.title.replace(/'/g, "\\'")}')">
            <div style="font-weight:600; margin-bottom:4px;">${n.title}</div>
            <div style="display:flex; justify-content:space-between; align-items:center;">
                <small>${n.createdAt}</small>
                <small style="text-decoration:underline;">View full notification</small>
            </div>
        </div>
    `).join('');
}

function readSingleNotif(notifId, title) {
    postToCSharp({ action: 'markNotifRead', userId: currentUser.id, notifId });
    showToast(title);
}

function markAllNotifsRead() {
    postToCSharp({ action: 'markAllNotifsRead', userId: currentUser.id });
}

// ================= CHAT DRAWER =================
function toggleChatDrawer(forceState) {
    closeMenus();
    const drawer = document.getElementById('chat-drawer');
    const backdrop = document.getElementById('chat-backdrop');
    const shouldOpen = typeof forceState === 'boolean' ? forceState : !drawer.classList.contains('open');

    drawer.classList.toggle('open', shouldOpen);
    backdrop.classList.toggle('open', shouldOpen);

    if (shouldOpen && currentUser) {
        postToCSharp({ action: 'getContacts', userId: currentUser.id });
        renderChatDrawer();
    }
}

function renderChatDrawer() {
    const body = document.getElementById('chat-drawer-body');
    if (!body) return;

    const search = (document.getElementById('chat-search')?.value || '').toLowerCase();

    // Хэрэв хэн нэгэнтэй чат нээсэн байвал зурвасын цонхыг харуулна
    if (activeChatPartner) {
        body.innerHTML = `
            <div style="padding:10px 14px; background:#f1f5f9; border-bottom:1px solid var(--border); display:flex; align-items:center; gap:10px;">
                <button style="background:none; border:none; font-size:16px; cursor:pointer; color:var(--green-dark);" onclick="closeActiveChat()">←</button>
                <div class="avatar" style="width:30px; height:30px; font-size:11px;">${getInitials('', activeChatPartner.name)}</div>
                <div>
                    <div style="font-weight:700; font-size:13.5px;">${activeChatPartner.name}</div>
                    <div style="font-size:11px; color:var(--green-main);">● ${activeChatPartner.role}</div>
                </div>
            </div>
            <div class="chat-messages" id="chat-msg-list">
                ${chatMessages.length === 0 ?
                `<div style="text-align:center; color:var(--muted); font-size:12.5px; margin-top:20px;">Зурвас бичиж харилцаагаа эхлүүлнэ үү.</div>` :
                chatMessages.map(m => {
                    const isMe = m.senderId === currentUser.id;
                    return `
                        <div class="msg-bubble ${isMe ? 'msg-me' : 'msg-other'}">
                            <div>${m.text}</div>
                            <div style="font-size:10px; opacity:0.75; margin-top:3px; text-align:right;">${m.time}</div>
                        </div>`;
                }).join('')
            }
            </div>
            <div class="chat-input-bar">
                <input type="text" id="chat-input-text" placeholder="Зурвас бичих..." onkeydown="if(event.key==='Enter') sendChatMessage()">
                <button class="btn-primary" style="padding:8px 14px;" onclick="sendChatMessage()">➤</button>
            </div>
        `;

        const msgBox = document.getElementById('chat-msg-list');
        if (msgBox) msgBox.scrollTop = msgBox.scrollHeight;
        return;
    }

    // Харилцагчдын жагсаалт (Starred / Private)
    const filtered = chatContacts.filter(c => c.name.toLowerCase().includes(search));

    body.innerHTML = `
        <div style="padding:10px 16px; font-size:12.5px; font-weight:700; color:var(--muted); background:#f8fafc; border-bottom:1px solid var(--border);">
            👤 Contacts (${filtered.length})
        </div>
        <div style="padding:10px 16px; font-size:13px; font-weight:600; color:#334155;">▾ Private (${filtered.length})</div>
        ${filtered.length === 0 ?
            `<div style="padding:20px; text-align:center; color:var(--muted); font-size:13px;">Хэрэглэгч олдсонгүй.</div>` :
            filtered.map(c => `
                <div class="chat-contact" onclick="openChatWith('${c.id}', '${c.name.replace(/'/g, "\\'")}', '${c.role}')">
                    <div class="avatar">${getInitials('', c.name)}</div>
                    <div style="flex:1;">
                        <div style="font-weight:600; font-size:13.5px; color:#0f172a;">${c.name}</div>
                        <div style="font-size:11.5px; color:var(--muted);">${c.role}</div>
                    </div>
                    <span style="color:var(--muted);">›</span>
                </div>
            `).join('')
        }
    `;
}

function openChatWith(partnerId, name, role) {
    activeChatPartner = { id: partnerId, name, role };
    chatMessages = [];
    renderChatDrawer();
    postToCSharp({ action: 'getMessages', userId: currentUser.id, partnerId });
}

function closeActiveChat() {
    activeChatPartner = null;
    renderChatDrawer();
}

function sendChatMessage() {
    const input = document.getElementById('chat-input-text');
    const text = input.value.trim();
    if (!text || !activeChatPartner) return;

    input.value = '';
    postToCSharp({
        action: 'sendMessage',
        senderId: currentUser.id,
        senderName: `${currentUser.lastName} ${currentUser.firstName}`.trim(),
        receiverId: activeChatPartner.id,
        text
    });
}