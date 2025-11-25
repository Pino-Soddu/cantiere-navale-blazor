// Funzione per lo scroll automatico della chat
function scrollToBottom(element) {
    element.scrollTop = element.scrollHeight;

    // 👇 Questa riga rende la funzione chiamabile da C#
    window.scrollToBottom = scrollToBottom;
}