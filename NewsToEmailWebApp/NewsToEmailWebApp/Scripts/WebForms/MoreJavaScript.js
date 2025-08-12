function HelpMeChecked(){
    var hm = document.getElementsByClassName("help-me");
    var cb = document.getElementById("HelpMeCB");
    if (cb.getAttribute("checked")) {
        cb.removeAttribute("checked");
        
        for (var i = 0; i < hm.length; i++) {
            hm[i].style.visibility = "hidden";
            hm[i].style.height = "0";
            hm[i].style.width = "0";
        }
    }
    else {
        cb.setAttribute("checked", "checked");
        for (var i = 0; i < hm.length; i++) {
            hm[i].style.visibility = "visible";
            hm[i].style.height = "auto";
            hm[i].style.width = "auto";
        }
    }
}

function CheckHideUnhideClass(cssClass) {
    var hm = document.getElementsByClassName(cssClass);
    var cb = document.activeElement;
    if (cb.getAttribute("checked")) {
        cb.removeAttribute("checked");

        for (var i = 0; i < hm.length; i++) {
            hm[i].style.visibility = "hidden";
            hm[i].style.height = "0";
            hm[i].style.width = "0";
        }
    }
    else {
        cb.setAttribute("checked", "checked");
        for (var i = 0; i < hm.length; i++) {
            hm[i].style.visibility = "visible";
            hm[i].style.height = "auto";
            hm[i].style.width = "auto";
        }
    }
}
