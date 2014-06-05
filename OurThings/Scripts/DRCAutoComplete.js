$(function () {

    $("#tbcustselect").autocomplete({
        source: function (request, response) {
            $.ajax({
                url: "/DRCCustomer/findnames", type: "POST", dataType: "json"
                    , data: { searchText: request.term, maxResults: 10 },
                success: function (data) {
                    response($.map(data, function (item) {
                        return { label: item.AcctName, value: item.AcctName, id: item.AcctID }

                    }))
                }

            })

        },
        select: function (event, ui) {
            $.ajax({
                url: "/DRCCustomer/custsel", type: "POST", dataType: "json", data: { acctid: ui.item.id }
//       , success: function () { $.post("/DRCCustomer/custsel", { acctid: ui.item.id }, function (data) { $("#tryme").val(data.Name); }, "json") }



            })

        }

    });

});