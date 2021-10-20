var _onSubmit = false;
//visualizza un alert customizzato()
window.alert = function (msg) {
    $('#alert-content').html(msg);  //richiama l'elemento p con quell'id e gli passa una stringa
    $('#alert-div').css('top', '0px');  //visualizza il div con quell'id all'interno dell'header
    setTimeout(function () { $('#alert-div').css('top', '-300px'); }, 6000);    //definisce lo stile e il tempo di animazione
    $('#alert-div').attr('tabindex', '0');       //da chiedere
    $('#alert-div').focus();
    $('#alert-div').attr('tabindex', '-1');
};

$(document).ready(function () {
    init(); //funzione eseguita non appena viene caricato il DOM
});

$(document).ready(function () {
    var current_fs, next_fs, previous_fs;
    var opacity;
    //invoco l'evento click sull'elemento con la classe next
    $(".next").click(function () {
        current_fs = $(this).parent();  //assegno la var a tutti gli elementi padre (posizionati sopra) dell'elemento con la classe next  
        next_fs = $(this).parent().next();  //restituisce la classe next a tutti gli elementi di pari livello successivi(in questo caso agli elementi li)
        if (validateForm(current_fs.attr('id'))) {
            $('#' + current_fs.attr('id') + ' :input').removeClass("valid");
            $('#' + current_fs.attr('id') + ' :input').removeClass("invalid");
            //se viene verificata la condizione, il selettore eq() aggiunge la classe all'elemento li successivo con quell' id 
            $("#progressbar li").eq($("fieldset:not(.ajax-modal-content fieldset)").index(next_fs)).addClass("active").css("color", "green");
            $("#progressbar li").eq($("fieldset:not(.ajax-modal-content fieldset)").index(next_fs)).focus();
            $("#progressbar li").eq($("fieldset:not(.ajax-modal-content fieldset)").index(current_fs)).addClass("active").css("color", "blue");
            $("#progressbar li").eq($("fieldset:not(.ajax-modal-content fieldset)").index(previous_fs)).focus();

            next_fs.show();     //se true => veiene visualizzato il secondo fieldset
            current_fs.animate({ opacity: 0 }, {
                step: function (now) {
                    opacity = 1 - now;
                    current_fs.css({
                        'display': 'none',
                        'position': 'relative'
                    });
                    next_fs.css({ 'opacity': opacity });
                },
                duration: 600, complete: function () { $('html,body').animate({ scrollTop: 0 }, 300); }
            });
        }
    });
    $(".nextTwoSteps").click(function () {
        current_fs = $(this).parent();  //assegno la var a tutti gli elementi padre (posizionati sopra) dell'elemento con la classe next  
        next_fs = $("#msform-field-4");  //dichiaro e inizializzo la var con l'id del field4
        if (validateForm(current_fs.attr('id'))) {
            //$('#' + current_fs.attr('id') + ' :input').removeClass("valid");
            //$('#' + current_fs.attr('id') + ' :input').removeClass("invalid");
            //se viene verificata la condizione, il selettore eq() aggiunge la classe all'elemento li successivo con quell' id 
            $("#progressbar li").eq($("fieldset:not(.ajax-modal-content fieldset)").index(next_fs)).addClass("active").css("color", "green");
            $("#progressbar li").eq($("fieldset:not(.ajax-modal-content fieldset)").index(next_fs)).focus();
            $("#progressbar li").eq($("fieldset:not(.ajax-modal-content fieldset)").index(current_fs)).addClass("active").css("color", "blue");
            $("#progressbar li").eq($("fieldset:not(.ajax-modal-content fieldset)").index(previous_fs)).focus();

            next_fs.show("#msform-field-4");     //se true => veiene visualizzato il quarto fieldset
            current_fs.animate({ opacity: 0 }, {
                step: function (now) {
                    opacity = 1 - now;
                    current_fs.css({
                        'display': 'none',
                        'position': 'relative'
                    });
                    next_fs.css({ 'opacity': opacity });
                },
                duration: 600, complete: function () { $('html,body').animate({ scrollTop: 0 }, 300); }
            });
        }
    });
    $(".previous").click(function () {
        //$("#divRiepilogo").css({ "visibility": 'hidden', "display": 'none' })
        current_fs = $(this).parent();
        previous_fs = $(this).parent().prev();
        $("#progressbar li").eq($("fieldset:not(.ajax-modal-content fieldset)").index(current_fs)).removeClass("active").css("color", "grey");
        $("#progressbar li").eq($("fieldset:not(.ajax-modal-content fieldset)").index(previous_fs)).focus();
        previous_fs.show();
        current_fs.animate({ opacity: 0 }, {
            step: function (now) {
                opacity = 1 - now;
                current_fs.css({
                    'display': 'none',
                    'position': 'relative'
                });
                previous_fs.css({ 'opacity': opacity });
            },
            duration: 600, complete: function () { $('html,body').animate({ scrollTop: 0 }, 300); }

        });
    });
    $('.radio-group .radio').click(function () {
        $(this).parent().find('.radio').removeClass('selected');
        $(this).addClass('selected');
    });
    $(".submit").click(function () {
        return false;
    });
});

function init() {
    //inizializza lapagina: binidings e init di elementi bootstrap o altro
    $('.material-icons').css('opacity', '1');  //definisce l'opacità della freccia che appare quando lo scroll > top di 60px
    $(document).scroll(function () {
        if ($(this).scrollTop() > 60) { //se la distanza dal top è maggiore di 60px
            $('#scroll').fadeIn();  //visualizza l'icona
        }
        else {
            $('#scroll').fadeOut(); //altrimenti => nasconde l'icona quando la distanza è inferiore a 60px
        }
    });
    //gestisce il comportamento dell'icona di tipo scroll
    $('#scroll').on('click', function (evt) {    //
        evt.stopImmediatePropagation();  //(stoppa l'esecuzione di altri eventi dopo il click sull'icona scroll) funziona anche senza=>  ?     
        $('html,body').animate({ scrollTop: 0 }, 600); return false; //se true(scroll cliccato) imposta il tempo di scroll della pagina
    });
    //associa l'evento click all'elemento anchor con classe form-submit(conferma e vota)
    $('body a.form-submit').on('click', function (evt) {

        //se true(controllo validazione superato)
        if (validateForm($(this).attr("data-form"))) {
            //rimuove le classi dai tag a con attr data-form e da tutti gli input 
            $('#' + $(this).attr("data-form") + ' :input').removeClass("valid");
            $('#' + $(this).attr("data-form") + ' :input').removeClass("invalid");
            if (!_onSubmit) {
                _onSubmit = true;
                setTimeout(function () { _onSubmit = false; }, 500);
                document.getElementById($(this).attr("data-form")).submit();
            }
        }
    });
    $('body .submit-on-enter').on('keydown', function (evt) {
        if (evt.which == 13 || evt.keyCode == 13) {
            if (validateForm($(this).attr("data-form"))) {
                $('#' + $(this).attr("data-form") + ' :input').removeClass("valid");
                $('#' + $(this).attr("data-form") + ' :input').removeClass("invalid");
                if (!_onSubmit) {
                    _onSubmit = true;
                    setTimeout(function () { _onSubmit = false; }, 500);
                    document.getElementById($(this).attr("data-form")).submit();
                }
            }
        }
    });
    $('body input[type="checkbox"]').on('change', function () {
        if (this.checked) {
            $('#' + this.id.replace('_check', '')).val(1);
        }
        else { $('#' + this.id.replace('_check', '')).val(0); }
    });
    $('.noctrlv').on('paste', function (e) { e.preventDefault(); });
}

function twoback() {
    var candidati = $('#confirmWhiteCard');
    //$('#btnRiepilogo').removeClass("previous");
    //$('#btnRiepilogo').removeClass("previousTwoSteps");

    if (candidati.val() == 1) {

        candidati.val(0);
        resetbox();

        current_fstwo = $("#msform-field-4");
        previous_fstwo = $("#msform-field-2");

        previous_fstwo.show("#msform-field-2");
        current_fstwo.animate({ opacity: 0 }, {
            step: function (now) {
                opacity = 1 - now;
                current_fstwo.css({
                    'display': 'none',
                    'position': 'relative'
                });
                previous_fstwo.css({ 'opacity': opacity });
            },
            duration: 600, complete: function () { $('html,body').animate({ scrollTop: 0 }, 300); }
        });
        /* inizializzo i tooltips BS */
        $(function () {
            $('[data-toggle="tooltip"]').tooltip()
        })
    }
    else {
        //prende tutte le checkbox del div con quell'id
        var checks = $("#ajax-candidati-target :checkbox");
        //cicla le checkbox
        for (let i = 0; i < checks.length; i++) {
            //
            $(checks[i]).prop("checked", false).prop("disabled", false);
        }

        resetbox();
        current_fstwo = $("#msform-field-4");
        previous_fstwo = $("#msform-field-3");

        previous_fstwo.show("#msform-field-3");
        current_fstwo.animate({ opacity: 0 }, {
            step: function (now) {
                opacity = 1 - now;
                current_fstwo.css({
                    'display': 'none',
                    'position': 'relative'
                });
                previous_fstwo.css({ 'opacity': opacity });
            },
            duration: 600, complete: function () { $('html,body').animate({ scrollTop: 0 }, 300); }
        });
    }
}

function ajaxCheck(id, url, html) {
    //funzione richiamata dai pulsanti next del form multistep
    //effettua una chiamata verso il controller ajax per verificare cose prima di proseguire
    //id: suffisso degli id interessati
    //url: actionresult ajax da richiamare
    //ritorna javascript che viene eseguito
    var noCache = Math.floor(Math.random() * 1111);
    $('#' + id + '-check').fadeOut(200, function () {
        $('#' + id + '-loader').fadeIn(200, function () {
            var xmlhttp;
            if (window.XMLHttpRequest) { xmlhttp = new XMLHttpRequest(); }
            else { xmlhttp = new ActiveXObject("Microsoft.XMLHTTP"); }
            xmlhttp.onreadystatechange = function () {
                if (xmlhttp.readyState == 4 && xmlhttp.status == 200) {
                    $('#' + id + '-loader').fadeOut(200, function () {
                        $('#' + id + '-check').fadeIn(200, function () {
                            if (html) {
                                $('#' + id + '-target').html(xmlhttp.responseText);
                                $('#' + id + '-trigger').trigger('click');
                            }
                            else {
                                eval(xmlhttp.responseText);
                            }
                        });
                    });
                }
            }
            //xmlhttp.open("GET", (url + (url.indexOf('?') != -1 ? '&' : '?') + 'buff=' + noCache).replace(/&amp;/g, "&"), true);
            //xmlhttp.send();

            xmlhttp.open('GET', ('/Home/' + url + (url.indexOf('?') != -1 ? '&' : '?') + 'buff=' + noCache).replace(/&amp;/g, "&"), true);
            //permette di inviare dati al server
            xmlhttp.setRequestHeader("content-type", "application/json; charset=utf-8");
            xmlhttp.send();
        });
    });
}

function launchAjaxModal(suff, url, show, appendForm) {
    //apre una modale popolata via ajax, oppure integra con i risultati una modale già aperta
    //suff: suffisso dell'id
    //url: la url da richiamare via XMLHttpRequest
    //show: apre la modale oppure no
    //appendForm: se true, alla url viene accodata la serializzazione del form
    $('#' + suff + '-modal-content').off(); $('#' + suff + '-modal-content').find('*').off();
    $('#' + suff + '-modal-content').html('');
    $('#' + suff + '-modal-content').css('height', '100px');
    if (show) { $('#' + suff + '-modal').modal('show'); }
    url = (appendForm ? url + '?' + $('#' + suff + '-form').serialize() : url);
    $('#' + suff + '-modal-loader').fadeIn(200, function () {
        var xmlhttp;
        if (window.XMLHttpRequest) { xmlhttp = new XMLHttpRequest(); }
        else { xmlhttp = new ActiveXObject("Microsoft.XMLHTTP"); }
        xmlhttp.onreadystatechange = function () {
            //controlla lo stato della risposta
            if (xmlhttp.readyState == 4 && xmlhttp.status == 200) {
                $('#' + suff + '-modal-loader').fadeOut(200, function () {
                    $('#' + suff + '-modal-content').css('height', 'auto');
                    $('#' + suff + '-modal-content').html(xmlhttp.responseText);
                    setTimeout(function () {
                        $('[data-toggle="tooltip"]').tooltip();
                    }, 500);
                });
            }
        }
        xmlhttp.open("GET", (url).replace(/&amp;/g, "&"), true);
        xmlhttp.send();
    });
}

function launchConfirmModal(header, text, action) {
    //apre una modale di conferma dell'operazione
    //header: testo header della modale
    //text: testo nel corpo della modale
    //action: RedirectToAction da effettuare in caso di conferma
    $('#confirm-modal-header').html(header);
    $('#confirm-modal-body').html(text);
    $('#confirm-modal-confirm').attr("onClick", action);
    $('#confirm-modal').modal('show');
}

function validateForm(form) {
    //valida lato client la coerenza degli input. Si basa sulle classi degli input
    //form: id del form
    var validForm = true;
    var elements = $('#' + form + ' :input');
    for (f = 0; f < elements.length; f++) {
        var elemType = (elements[f].type).toUpperCase();
        if (elemType == 'TEXT' || elemType == 'NUMBER' || elemType == 'EMAIL' || elemType == 'TEXTAREA' || elemType == 'PASSWORD' || elemType == 'HIDDEN' || elemType == 'CHECKBOX' || elemType == 'FILE' || elemType.indexOf('SELECT') != -1) {
            var elemValue = $(elements[f]).val();   //setto la var con il valore contenuto/selezonato nell'elemento
            //se viene selezionato o valorizzato un campo => includilo nella var
            if (elemType.indexOf('SELECT') != -1) { elemValue = elements[f].options[elements[f].selectedIndex].value; }
            var isEmpty = (elemValue.replace(/\s/g, '') == '' ? true : false);  //
            $(elements[f]).removeClass("valid");
            $(elements[f]).removeClass("invalid");
            if ($(elements[f]).hasClass('integer')) {//numero intero
                //se è una stringa vuota e non è un intero                              //rimuovi                           //aggiungi
                if (!isEmpty && elemValue % 1 !== 0) { validForm = false; $(elements[f]).removeClass("valid"); $(elements[f]).addClass("invalid"); }
            }
            if ($(elements[f]).hasClass('required-check') && elemType == 'CHECKBOX') {//required checkbox
                if (!elements[f].checked) { validForm = false; $(elements[f]).removeClass("valid"); $(elements[f]).addClass("invalid"); }
            }
            if ($(elements[f]).hasClass('required') && elemType != 'CHECKBOX') {//required ***sempre ultima
                if (isEmpty) { validForm = false; $(elements[f]).removeClass("valid"); $(elements[f]).addClass("invalid"); }
            }
        }
    }
    if (!validForm) return window.alert('Prego selezionare una lista dall\'elenco oppure il pulsante scheda bianca');   //ritorna il parametro mess come strnga di errore
    return validForm;
}

function performAjaxCall(url, suff, html) {
    //operazioni generiche ajax, il response dai controller può essere un javascript (che viene eseguito), oppure html da inserire in un elemento
    //url: la url da richiamare via XMLHttpRequest
    //suff: il suffisso degli id degli elementi interessati (loader: la divisione con il preloader, container: la divisione che contiene il target, target: l'elemento da riempire)
    //html: boolean (true: il response è html da inserire in un elemento, false: il response è javascript da eseguire)
    //var noCache = Math.floor(Math.random() * 1111);
    $('#' + suff + '-container').fadeOut(200, function () {
        $('#' + suff + '-loader').fadeIn(200, function () {
            var xmlhttp;
            if (window.XMLHttpRequest) { xmlhttp = new XMLHttpRequest(); }
            else { xmlhttp = new ActiveXObject("Microsoft.XMLHTTP"); }
            xmlhttp.onreadystatechange = function () {
                if (xmlhttp.readyState == 4 && xmlhttp.status == 200) {
                    if (html) {
                        $('#' + suff + '-target').html(xmlhttp.responseText);
                        $('#' + suff + '-loader').fadeOut(200, function () { $('#' + suff + '-container').fadeIn(200); });
                    }
                    else { eval(xmlhttp.responseText); }
                }
            }
            xmlhttp.open("GET", (url + (url.indexOf('?') != -1 ? '&' : '?') + 'buff=' + noCache).replace(/&amp;/g, "&"), true);
            xmlhttp.send();
        });
    });
}
//funzione di servizio chiamata da altre funzioni per mostrare o nascondere un ID elemento
function manage(elemento, operazione) {
    var time = 600;
    var trans = 0.2;
    if (operazione == 'off') {                                 //specifica l'effetto di transizione rallentando alla fine
        $('#' + elemento).css({ "transition": 'all ' + trans + 's ease-out' })
        $('#' + elemento).hide(time)
        //$('#' + elemento).css({ "transition": '' })

    }
    else if (operazione == 'on') {
        $('#' + elemento).css({ "transition": 'all ' + trans + 's ease-out' })
        $('#' + elemento).show(time)
        //$('#' + elemento).css({ "transition": '' })
    }

    return true;
}
//function che restituisce la scelta dell'elettore, la scheda bianca o il voto nullo

function setValoreRadioLista(valore)
{
    $('#radioListe').val(valore);
    return;
}

function Riepilogo(visualizza) {
    // manage("fs-four-votoUfficiale", "off");
    manage("fs-four-boxWhiteCard", "off");
    manage("fs-four-boxNullCard", "off");

    switch (visualizza) {
        // Lista e candidati
        case "datiVoto":
            //invoco la function manage settando i parametri con l'id dell'elemento e il valore off => nasconde l'elemento
            manage("fs-four-boxWhiteCard", "off");
            manage("fs-four-boxNullCard", "off");
            //19/05/21 =>riga implementata poichè dopo aver scelto schedaBianca/votoNullo se tornavo indietro e sceglievo lista e candidati, questi ultimi non me li visualizzava nel riepuilogo
            manage("fs-four-votoUfficiale", "on");
            manage("Candi", "off");
            manage("NoCandi", "off");
            manage("tabellaRiepilogo", "off");

            let listaPartito = $(".test > input"); // contiene gli elementi input con i check delle liste
            let lblListaPartito = $(".test > label"); // contiene gli elementi label con i nomi delle liste

            for (var i = 0; i < listaPartito.length; i++) { //ciclo i radio
                if ($(listaPartito[i]).is(":checked")) { //se è fleggato
                    $("#testoA").append(
                        `<h3 id="spanList"> ${lblListaPartito[i].innerHTML}</h3>`);
                }
            }
            //prende tutti gli input fleggati
            let listaCandidati = $('#ajax-candidati-target input:checked');
            //array vuoto
            let pila = [];
            
            //scorro listaCandidati aggiungendo l'id alla pila
            for (var i = 0; i < listaCandidati.length; i++) {
                //ciclo e riempio l'array con l'id dei candidati fleggati
                var tr = $(listaCandidati[i]).parent().parent().parent();
                pila.push(tr);
            }
            manage("NoCandi", "on");
            manage("Candi", "off");
            manage("tabellaRiepilogo", "off");
            for (var i = 0; i < pila.length; i++) {
               
                
                $("#datiRiepilogo").html($("#datiRiepilogo").html() + '<tr>' + $(pila[i]).html() + '</tr>');

                if (pila.length != 0) {
                    manage("NoCandi", "off");
                    manage("Candi", "on");
                    manage("tabellaRiepilogo", "on");
                }
                $('#datiRiepilogo').find('.toremove').remove();
            }
            break;
        //scheda bianca => stessa formattazione fatta per liste e candidati 
        case "schedaBianca":
            manage("fs-four-votoUfficiale", "off");
            manage("fs-four-boxWhiteCard", "on");
            manage("fs-four-boxNullCard", "off");
            manage("boxNullCard", "off");
            $('#msform-field-2').hide();
            $("#progressbar li").eq($("fieldset:not(.ajax-modal-content fieldset)", 3).index(next_fs)).addClass("active").css("color", "green");
            $("#progressbar li").eq($("fieldset:not(.ajax-modal-content fieldset)").index(3)).focus();
            break;
        default:
    }
    //return;
}
// gestione dei pulsanti scheda bianca e nulla
function opzioniVoto(pulsante) {
    
    switch (pulsante) {
        case "btnSchedaBianca":
           
            manage("boxliste", "off")
            manage("boxInfoBackNavigation", "off")
            manage("boxInfoNextNavigation", "off")
            manage("Help-a-Sx", "off")
            manage("Help-a-Dx", "off")
            manage("comandiSchede", "off")
            manage("titleList", "off")
            manage("titleWhite", "on")
            $('#ajax-candidati-check').css({ "opacity": '0' })
            $('#ajax-candidati-check').addClass("disabled")
            $('#back').css({ "opacity": '0' })
            $('#back').addClass("disabled")
            manage("boxNullCard", "off")
            manage("boxWhiteCard", "on")
            break;
        case "btnSchedaBiancaNull":
            
            console.log($('#radioListe'));
            $('#ajax-candidati-check').removeClass("disabled")
            $('#ajax-candidati-check').css({ "opacity": '1' })
            $('#back').removeClass("disabled")
            $('#back').css({ "opacity": '1' })
            manage("boxliste", "on")
            manage("boxInfoBackNavigation", "on")
            manage("boxInfoNextNavigation", "on")
            manage("Help-a-Sx", "on")
            manage("Help-a-Dx", "on")
            manage("comandiSchede", "on")
            manage("titleList", "on")
            manage("titleWhite", "off")
            manage("boxWhiteCard", "off")
            manage("boxNullCard", "off")
            break;
        /* case conferma scheda bianca */
        case "btnSchedaNullaGo":           
            manage("boxliste", "off")
            manage("boxInfoBackNavigation", "off")
            manage("boxInfoNextNavigation", "off")
            manage("Help-a-Sx", "off")
            manage("Help-a-Dx", "off")
            manage("comandiSchede", "off")
            manage("titleList", "on")
            manage("titleWhite", "off")
            manage("boxWhiteCard", "off")
            break;
        case "btnSchedaNulla":
            manage("boxliste", "off")
            manage("Help-a-Sx", "off")
            manage("Help-a-Dx", "off")
            manage("comandiSchede", "off")
            manage("titleList", "off")
            manage("titleNull", "on")
            $('#ajax-candidati-check').css({ "opacity": '0' })
            $('#ajax-candidati-check').addClass("disabled")
            manage("boxWhiteCard", "off")
            manage("boxNullCard", "on")
            break;
        case "btnSchedaNullaNull":
            $('#ajax-candidati-check').removeClass("disabled")
            $('#ajax-candidati-check').css({ "opacity": '1' })
            manage("boxliste", "on")
            manage("Help-a-Sx", "on")
            manage("Help-a-Dx", "on")
            manage("comandiSchede", "on")
            manage("titleList", "on")
            manage("titleNull", "off")
            manage("boxNullCard", "off")
            break;
        default:
            alert("comando non riconosciuto in UI - funzione opzioniVoto()")
    }
    return true;
}

function resetbox() {
    manage("titleList", "on");
    manage("titleWhite", "off");
    manage("titleNull", "off");
    manage("boxWhiteCard", "off");
    manage("boxNullCard", "off");
    manage("fs-four-boxWhiteCard", "off");
    manage("fs-four-boxNullCard", "off");
    $("#spanList").remove();
    $("#Lista").empty();
    $("#Lista").detach("li");
    $('#ajax-candidati-check').removeClass("disabled")
    $('#ajax-candidati-check').css({ "opacity": '1' })
    $('#back').removeClass("disabled")
    $('#back').css({ "opacity": '1' })
    $("#boxliste").css('display', 'block');
    $("#Help-a-Sx").css('display', 'block');
    $("#Help-a-Dx").css('display', 'block');
    $("#comandiSchede").css('display', 'block');
    $("#comandiListe").css('display', 'block');
    $("#comandiListe").css('opacity', '1');
    $("#comandiListeVotoNullo").css('display', 'block');
    $("#comandiListeVotoNullo").css('opacity', '1');
    return;
};

function togli(elemento) {
    //$('#' + elemento).removeAttr("style");
    $('#' + elemento).css({ "display": 'block;' })
    alert(elemento)
    return true;
}

// per evitare il bug di un radio selezionato e poi seleziono scheda bianca
function pulisciradio()
    { 
    var radio = document.querySelector('input[type=radio][name=Partito]:checked');
    if (radio != "" || radio.isEmpty)
    {    
    radio.checked = false;
        radio.val(0);
        
    }

}

function ControlloValoreCandidati() {

    let counter = 1;
    //num checkbox presenti nel box
    let radios = $("#ajax-candidati-target :input");
    //var contenente un terzo dei candidati della lista

    let nome = $('#nomeComitato').text();
    
    //ciclo le check e verifico se sono fleggate
    for (var i = 0; i < radios.length; i++) {
        if (radios[i].checked == true) {
            //incremento la var counter di uno d ogni check
            counter += 1;
        }
    }
    //se sono state fleggate 3 check => num preferenze == 3
    if (counter > gino) {
        //ciclo le check
        for (var i = 0; i < radios.length; i++) {
            //se le altre check non sono fleggate
            if (radios[i].checked == false) {
                //disabilitale
                radios[i].disabled = true;
            }
        }
    }
    else {//se viene defleggata una delle tre checkbox 
        for (var i = 0; i < radios.length; i++) {
            //riabilita tutte le check che non sono fleggate
            radios[i].disabled = false;
        }
    }
}

/* funzione per gestire la conferma finale del voto */
function gameOver(decidi) {
    if (decidi == 0) {
        manage("btnRiepilogo", "off");
        manage("ConfermaVoto", "off");
        //AvvisoFinale
        manage("AvvisoFinale", "off");
        manage("ConfermaFinale", "on");
    }
    else if (decidi == 1) {
        manage("ConfermaFinale", "off");
        manage("AvvisoFinale", "on");
        manage("btnRiepilogo", "on");
        manage("ConfermaVoto", "on");
    }
    else {
        alert("Errore interno: controllo non riconosciuto");
        return false;
    }
    return;

}



