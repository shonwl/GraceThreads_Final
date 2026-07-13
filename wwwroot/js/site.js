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

async function sendChatMessage(event) {
    event.preventDefault();
    const input = document.getElementById('chat-input');
    const text = input.value.trim();
    if (!text) return;

    const messages = document.getElementById('chat-messages');
    
    // Clear initial greeting screen placeholder if it exists
    const emptyState = messages.querySelector('.flex-col.items-center.justify-center');
    if (emptyState) {
        messages.innerHTML = '';
    }

    // Append User Message Bubble
    const userBubble = document.createElement('div');
    userBubble.className = 'self-end max-w-[80%] bg-[#f05a1a] text-white text-sm px-4 py-2.5 rounded-sm font-medium break-words';
    userBubble.textContent = text;
    messages.appendChild(userBubble);

    // Clear input bar and scroll down
    input.value = '';
    messages.scrollTop = messages.scrollHeight;

    // Create a temporary "Thinking..." typing animation bubble
    const typingBubble = document.createElement('div');
    typingBubble.className = 'self-start max-w-[80%] bg-[#222222] text-[#888888] text-sm px-4 py-2.5 rounded-sm animate-pulse';
    typingBubble.textContent = "Analyzing dimensions...";
    messages.appendChild(typingBubble);
    messages.scrollTop = messages.scrollHeight;

    try {
        // Post data safely to our hidden Razor routing pipeline
        const response = await fetch('/ChatAPI', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ message: text })
        });

        // Remove the loading bubble
        messages.removeChild(typingBubble);

        if (response.ok) {
            const data = await response.json();
            
            // Append Gemini Sizing Recommendation
            const replyBubble = document.createElement('div');
            replyBubble.className = 'self-start max-w-[80%] bg-[#222222] text-[#cccccc] text-sm px-4 py-2.5 rounded-sm whitespace-pre-wrap break-words';
            replyBubble.textContent = data.reply;
            messages.appendChild(replyBubble);
        } else {
            throw new Error("API Connection Error");
        }
    } catch (error) {
        if (messages.contains(typingBubble)) messages.removeChild(typingBubble);
        
        const errorBubble = document.createElement('div');
        errorBubble.className = 'self-start max-w-[80%] bg-red-950/30 text-red-400 text-xs px-4 py-2.5 border border-red-900/50';
        errorBubble.textContent = "Unable to connect to sizing system. Please review your network status.";
        messages.appendChild(errorBubble);
    }

    messages.scrollTop = messages.scrollHeight;
}