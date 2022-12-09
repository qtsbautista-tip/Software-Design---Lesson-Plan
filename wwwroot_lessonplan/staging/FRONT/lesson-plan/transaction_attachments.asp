<!--#include file="head.asp"-->
<div class="container-fluid">
    <table>
    <tr>
        <td><h4>Module Code:</h4></td>
        <td><h4><span id="module_code"></span></h4></td>
        <td>&emsp;</td>
        <td><h4>Reference:</h4></td>
        <td><h4><span id="refno"></span></h4></td>
    </tr>
    </table>
    <div id="grid"></div>
</div>


<script src="transaction_attachments.js?t=<% Write(nocache_time) %>"></script>
<% FooterIndicator = False %>
<!--#include file="footer.asp"-->