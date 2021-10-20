// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

/* genera un VN per simulare la UI reale */
function mostraVN() {
    document.getElementById("VN").innerHTML = Math.floor(Math.random() * 100000000);
}

function copiaVN() {
    var temp = document.getElementById("VN").innerHTML;
    if (temp != "") {
        document.getElementById("validationNumber").value = temp;
        $("#validationNumber").css('text-align', 'center');
    }
    else
        alert("UI error");
}
//verifica il consenso dell'utente prima di reinderizzarlo sull'appb
$('#CheckVoto').click(function ()
{
    var bottoneVaiAlVoto = document.querySelector('button');
    if ($('#CheckVoto').is(':checked'))
    {
        //$('#VaiAlVoto').removeClass('disabled');
        bottoneVaiAlVoto.disabled = false;
    }
    else
    {
        //$('#VaiAlVoto').addClass('disabled')
        bottoneVaiAlVoto.disabled = true;
    }
});
//chiude il div contenente l'esempio per l'inserimento del codice elettore
$('#closeDivImg').click(function () {
    $('#fileImg').addClass('hidden');
});
//apre il div contenente l'esempio di come inserire il codice elettore
$('#openDivImg').click(function () {
    $('#fileImg').removeClass('hidden');
});

//controlla il formato del codice elettore
function SetCodicePratica(CodicePratica) {
    var modulo = document.getElementById("#ce");
    let CodiceElettoreclient = /^[EI]\/[0-9][0-9][0-9][0-9][0-9][0-9][0-9]\/[0-9][0-9][0-9][0-9][0-9][0-9]+$/;
    var OK = CodiceElettoreclient.test(CodicePratica.value);

    // test 
    if (OK) {
        modulo.submit();   //Superato il controllo client invia i dati al server  
    } else {
        $('#divWarnig').removeClass("hidden");
        event.preventDefault();
        return;
    }
    /*modulo.submit();*/
}
$(document).ready(function () {
    $('#test').html("");
    console.log(Intl.DateTimeFormat().resolvedOptions().timeZone);
});

//function SetCodicePratica(CodicePratica) {


//    let CodiceElettoreclient = /^[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]+$/;
//    var OK = CodiceElettoreclient.test(CodicePratica.value);

//    if (!OK) {
//        $('#divWarnig').removeClass("hidden");
//        event.preventDefault();
//        return;   //Superato il controllo client invia i dati al server  
//    }

//}




//$("#regex").submit(function (input) {
//    let regex = /^[0-9][0-9][0-9][0-9][0-9][0-9][0-9]-COM-[0-9][0-9][0-9][0-9][0-9][0-9][0-9]+$/;
//    alert(regex);
//    return regex.test(input);
//});





