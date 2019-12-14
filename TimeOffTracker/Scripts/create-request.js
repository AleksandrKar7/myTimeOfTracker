var form = document.getElementById('verificationRequestForm');
var submitBtn = document.getElementById('submitBtn');
var from = document.getElementById('VacationRequest_DateStart');
var to = document.getElementById('VacationRequest_DateEnd');

form.addEventListener('click', checkFormValidity);

from.addEventListener('change', () => {
    to.setAttribute('min', from.value);
});
to.addEventListener('change', () => {
    from.setAttribute('max', to.value);
});

$("#requestModal [data-dismiss=modal]").bind('click', () => {
    $('#requestModal').hide();
    window.location.href = '/Home';
})

submitBtn.addEventListener('click', () => {
    if (!form.checkValidity()) {
        return;
    }
    var requestData = createRequestData(form.id);
    console.log(requestData);

    $.ajax({
        url: '/VacationRequest/CreateVacationRequest',
        dataType: "json",
        contentType: "application/json",
        method: "POST",
        data: JSON.stringify(requestData),
        success: function (responce) {
            console.log(requestData);
            $('#requestModal').modal('show');
        },
        error: function (error) {
            var errorBlock = document.getElementById('textError');
            errorBlock.innerHTML = "Incorrect data or you have spent the limit for this type of vacation.";

            $('#errorModal').modal('show');
        }
    });
});

function createRequestData(id) {
    id = '#' + id;
    const request =  $(id).serializeArray().reduce(function(obj, item) {
        var part = obj;
        console.log('name: ', item.name, 'value: ', item.value);
        var parts = item.name.split('.');
        if (item.name === 'ApproversId') {
            if(!obj[item.name]) {
                obj[item.name] = [];
            }
            obj[item.name].push(item.value);
            return obj;
        }
       
        if (item.name.indexOf('VacationRequest.Date') !== -1) {
            item.value = new Date(item.value);
        }
        
        for (let i = 0; i < parts.length - 1; i++) {
            if (!part[parts[i]]) {
                part[parts[i]] = {};
            }
            part = obj[parts[i]];
        }
        part[parts[parts.length - 1]] = item.value;
        return obj;
    }, {});

    const requestApprovers = [];
    request.ApproversId = [];
    ChooseApproversHelper.approvers.forEach((approver) => {
        if (approver.selected) {
            requestApprovers.push(approver);
        }
    });

    requestApprovers.sort((a, b) => a.priority - b.priority);

    requestApprovers.forEach((approver) => request.ApproversId.push(approver.Id));

    return request;

}
function checkFormValidity() {
    form.checkValidity() && ChooseApproversHelper.selectedApprovers.childNodes.length !== 0 ? submitBtn.disabled = false : submitBtn.disabled = true;
}



class ChooseApproversHelper {
    static initHelper(approvers, searchApprover, approversList, selectedApprovers) {
        this.approvers = approvers;
        this.searchApprover = document.getElementById(searchApprover);
        this.approversList = document.getElementById(approversList);
        this.selectedApprovers = document.getElementById(selectedApprovers);
        this.priority = 1;
        ChooseApproversHelper.config();
    }

    static initApprovers() {
        const selectedApprovers = [];
        this.approversList.innerHTML = '';
        this.selectedApprovers.innerHTML = '';
        this.approvers.forEach((approver) => {
            if (approver.selected) {
                selectedApprovers.push(approver);
            } else {
                this.approversList.append(new NotSelectedApprover('p', approver).node);
            }
        });
        selectedApprovers.sort((a, b) => a.priority - b.priority);
        selectedApprovers.forEach((selectedApprover) => this.selectedApprovers.append(new SelectedApprover('p', selectedApprover).node));
        console.log(ChooseApproversHelper.approvers);
    }

    static config() {
        this.searchApprover.addEventListener('focus', () => {
            changeVisibility(this.approversList, 'visible');
        });
        this.searchApprover.addEventListener('focusout', () => {
            setTimeout(() => {
                changeVisibility(this.approversList, 'hidden');
            }, 150);
        });
        this.searchApprover.addEventListener('input', () => this.filterApprovers());
    }

    static filterApprovers() {
        this.approversList.innerHTML = '';
        this.approvers
            .filter((approver) => !approver.selected && approver.Email.indexOf(this.searchApprover.value) !== -1)
            .forEach((approver) => this.approversList.append(new NotSelectedApprover('p', approver).node));
    }

}

class Approver {
    constructor(typeOfNode, approver) {
        this.node = document.createElement(typeOfNode);
        this.node.innerText = approver.Email;
    }

    onNodeClick() {
        ChooseApproversHelper.initApprovers();
        ChooseApproversHelper.searchApprover.value = '';
    }
}

class NotSelectedApprover extends Approver {
    constructor(typeOfNode, approver) {
        super(typeOfNode, approver);
        this.node.classList.add('approversItem');

        this.node.onclick = () => {
            approver.selected = true;
            approver.priority = ChooseApproversHelper.priority++;
            this.onNodeClick();
        }
    }
}

class SelectedApprover extends Approver {
    constructor(typeOfNode, approver) {
        super(typeOfNode, approver);
        this.node.classList.add('selected-approver');

        this.node.onclick = () => {
            approver.selected = false;

            ChooseApproversHelper.approvers.forEach((selectedApprover) => {
                if (selectedApprover.selected && selectedApprover.priority > approver.priority) {
                    selectedApprover.priority--;
                }
            });

            approver.priority = -1;

            ChooseApproversHelper.priority--;
            
            this.onNodeClick();
        }
    }
}

function changeVisibility(element, visibility) {
    element.style.visibility = visibility;
}

function chooseApprovers(approvers) {
    ChooseApproversHelper.initHelper(approvers, 'searchApprover', 'approversList', 'selectedApprovers');
    ChooseApproversHelper.initApprovers();
}
