﻿var dataTable;
$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("inprocess")) {
        LoadDataTable("inprocess");
    } else {
        if (url.includes("pending")) {
            LoadDataTable("Pending");
        } else {
            if (url.includes("completed")) {
                LoadDataTable("completed");
            } else {
                if (url.includes("approved")) {
                    LoadDataTable("approved");
                } else {
                    LoadDataTable("all");
                }
            }

        }

    }

});


function LoadDataTable(status) { 
   dataTable= $('#tblData').DataTable({
        "ajax": {url:'/areas/order/getall?status='+status}
         ,"columns": [
             { data: 'id',"width":"5%" },
             { data: 'name', "width": "15%" },
             { data: 'phoneNumbeer', "width": "20%" },
             { data: 'applicationUser.email', "width": "20%" },
             { data: 'orderStatus', "width": "15%" },
             { data: 'orderTotal', "width": "15%" },
             {
                 data: 'id', "render": function (data) {
                        return `<div class="w-75 btn-group" role="group">  
                        <a href="/areas/order/details?orderId=${data}" class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i></a>
                        </div>`},"width": "25%" }]
    });
}
