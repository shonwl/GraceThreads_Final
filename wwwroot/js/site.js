// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function toggleCart() {
    document.getElementById('cart-panel').classList.toggle('open');
    document.getElementById('cart-overlay').classList.toggle('open');
    updateChatWidgetVisibility();
}

function toggleChat() {
    document.getElementById('chat-panel').classList.toggle('open');
    document.getElementById('chat-overlay').classList.toggle('open');
    updateChatWidgetVisibility();
}

function updateChatWidgetVisibility() {
    const cartPanel = document.getElementById('cart-panel');
    const chatPanel = document.getElementById('chat-panel');
    const chatWidget = document.getElementById('chat-widget-container');
    
    // Only run if the chat widget actually exists on the page
    if (chatWidget && cartPanel && chatPanel) {
        if (cartPanel.classList.contains('open') || chatPanel.classList.contains('open')) {
            chatWidget.classList.add('opacity-0', 'pointer-events-none');
        } else {
            chatWidget.classList.remove('opacity-0', 'pointer-events-none');
        }
    }
}

function sendChatMessage(event) {
        event.preventDefault();
        const input = document.getElementById('chat-input');
        const text = input.value.trim();
        if (!text) return;

        const messages = document.getElementById('chat-messages');
        const emptyState = messages.querySelector('.flex-col.items-center.justify-center');
        if (emptyState) {
        messages.innerHTML = '';
        }

        const userBubble = document.createElement('div');
        userBubble.className = 'self-end max-w-[80%] bg-[#f05a1a] text-white text-sm px-4 py-2.5';
        userBubble.textContent = text;
        messages.appendChild(userBubble);

        input.value = '';
        messages.scrollTop = messages.scrollHeight;

        setTimeout(() => {
        const replyBubble = document.createElement('div');
        replyBubble.className = 'self-start max-w-[80%] bg-[#222222] text-[#cccccc] text-sm px-4 py-2.5';
        replyBubble.textContent = "Thanks for your question! Our team will follow up shortly — for anything urgent, use the Contact page.";
        messages.appendChild(replyBubble);
        messages.scrollTop = messages.scrollHeight;
        }, 600);
        }