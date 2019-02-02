function noBack() {
    disableBackButton();
    window.onload = disableBackButton();
    window.onpageshow = function (evt) { if (evt.persisted) disableBackButton(); };
    window.onunload = function () { void (0); };
}

function disableBackButton() {
    window.history.forward(1);
}  
