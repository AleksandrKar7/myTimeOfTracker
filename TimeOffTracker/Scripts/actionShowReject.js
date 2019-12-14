//Add to bundles

let parent = document.getElementById('parent')

parent.addEventListener('click', function (event) {

    console.log(event)

    if (event.target.matches('.trigger-btn')) {
        event.preventDefault()

        let point = event.target.parentElement;

        // Show form and cancel btn
        $(point).children('.form-reject-request').css('display', 'block');
        $(point).children('.btn-cancel-reject').css('display', 'block');

        // Hide reject button
        $(point).children('#reject-btn').css('display', 'none');
    }
    else if (event.target.matches('.btn-cancel-reject')) {
        event.preventDefault()
        let point = event.target.parentElement;
        $(point).children('.form-reject-request').css('display', 'none');
        $(point).children('.btn-cancel-reject').css('display', 'none');
        $(point).children('#reject-btn').css('display', 'block');
    }
    
})

function cancelReject() {

    document.getElementById("reject-btn").style.display = "block";
    document.getElementById("form-reject-request").style.display = "none";
    document.getElementById("btn-cancel-reject").style.display = "none";
}