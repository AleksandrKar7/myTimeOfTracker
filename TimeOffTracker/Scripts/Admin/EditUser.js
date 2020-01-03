

var checkbox = document.getElementById("IsChangePassword");
var div = document.getElementById("DivNewPassword");
function change() {
    if (checkbox.checked === true) {
        div.hidden = "";
    }
    if (checkbox.checked === false) {
        div.hidden = "hidden";
    }
};
