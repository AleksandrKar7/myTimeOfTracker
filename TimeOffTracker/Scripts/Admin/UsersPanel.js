
$(document).ready(
    //Авто-загрузка первой страницы
    function () {
    page = document.getElementById("page");
    var pageNum = page.value;
    if (pageNum == "0") {
        $("#results").load("GetPartOfUsers");
        pageNum++;
        page.value = pageNum;
    }

    //Скрипт для кнопки Show More
    document.body.onclick = function (e) {
        e = e || event;
        var target = e.target || e.srcElement;
        if (target.id == 'next') {

            pageNum++;
            page.value = pageNum;

        }
    }
});