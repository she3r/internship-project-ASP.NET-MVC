var tempDeliveries = [];
function saveTempDelivery(name, weight) {
    tempDeliveries.push({ "name": name, "weight": weight });
}

function incrementPageChoice() {
    let currPageItem = parseInt($("#page-choice").val());
    if (currPageItem < @numPages) {
        $("#page-choice").val(currPageItem + 1)
    }

}
function decreasePageChoice() {
    let currPageItem = parseInt($("#page-choice").val());
    if (currPageItem > 1) {
        $("#page-choice").val(currPageItem - 1)
    }
}
function addTemporaryDelivery() {
    var inputWeight = $("#delivery-weight").val();
    var inputName = $("#delivery-name").val();
    $("#delivery-weight").value = 0;
    $("#delivery-name").value = 0;

    var tbodyRef = document.getElementById('deliveryTable').getElementsByTagName('tbody')[0];
    var newRow = tbodyRef.insertRow();

    var newCellID = newRow.insertCell();
    var newCellName = newRow.insertCell();
    var newCellDate = newRow.insertCell();
    var newCellWeight = newRow.insertCell();
    var newCellEdition = newRow.insertCell();

    var textNodeID = document.createTextNode("nowy!");
    var textNodeName = document.createTextNode(inputName);
    var textNodeDate = document.createTextNode("teraz!");
    var textNodeWeight = document.createTextNode(inputWeight);
    var textNodeEdition = document.createTextNode("Zatwierdz aby edytowac");

    newCellID.appendChild(textNodeID);
    newCellName.appendChild(textNodeName);
    newCellDate.appendChild(textNodeDate);
    newCellWeight.appendChild(textNodeWeight);
    newCellEdition.appendChild(textNodeEdition);

    saveTempDelivery(inputName, inputWeight);
}
$("#delivery-post-all").on('submit', function () {
/*    debugger;*/
    $("#json-temp-deliveries").value = JSON.stringify(tempDeliveries, ['name', 'weight']);
    return true;
})
