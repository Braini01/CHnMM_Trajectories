<%@ Page Title="About Us" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="InputData.aspx.cs" Inherits="GestureCollectingInterface.InputData" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no" />
<link href="Styles/TouchStyle.css" rel="stylesheet" type="text/css" />
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h2>Input Data</h2>
    <asp:Label ID="lblUsername" runat="server" Text="Username: "></asp:Label>
    <asp:DropDownList ID="cbUsers" runat="server" DataSourceID="dsAllUsers" 
        DataTextField="Username" DataValueField="Id" AutoPostBack="True"></asp:DropDownList>

    <asp:Label ID="lblGesture" runat="server" Text="Gesture: "></asp:Label>
    <asp:DropDownList ID="cbGestures" runat="server" DataSourceID="dsUserGestures" 
        DataTextField="Name" DataValueField="Id">
    </asp:DropDownList>

    <span id="lblTraceCount"></span>

    <asp:LinqDataSource ID="dsUserGestures" runat="server" 
        ContextTypeName="LfS.GestureDatabase.dbEntities" EntityTypeName="" 
        Select="new (Id, Name)" TableName="Gestures" Where="User.Id == @User">
        <WhereParameters>
            <asp:ControlParameter ControlID="cbUsers" Name="User" 
                PropertyName="SelectedValue" Type="Int32" />
        </WhereParameters>
    </asp:LinqDataSource>

    <asp:EntityDataSource ID="dsAllUsers" runat="server" 
        ConnectionString="name=dbEntities" DefaultContainerName="dbEntities" 
        EnableFlattening="False" EntitySetName="Users" EntityTypeFilter="User" 
        Select="it.[Username], it.[Id]">
    </asp:EntityDataSource>

    <!--div id="touchField">
    <table id="keyTable"><tr><td id="Td1">S1</td><td id="Td2">S2</td><td id="Td3">S3</td><td id="Td4">S4</td><td id="Td5">S5</td><td id="Td6">S6</td><td id="Td7">S7</td></tr><tr><td id="Td8">S8</td><td rowspan="1" colspan="1" class="key" id="Td9">1</td><td id="Td10">S9</td><td rowspan="1" colspan="1" class="key" id="Td11">2</td><td id="Td12">S10</td><td rowspan="1" colspan="1" class="key" id="Td13">3</td><td id="Td14">S11</td></tr><tr><td id="Td15">S12</td><td id="Td16">S13</td><td id="Td17">S14</td><td id="Td18">S15</td><td id="Td19">S16</td><td id="Td20">S17</td><td id="Td21">S18</td></tr><tr><td id="Td22">S19</td><td rowspan="1" colspan="1" class="key" id="Td23">4</td><td id="Td24">S20</td><td rowspan="1" colspan="1" class="key" id="Td25">5</td><td id="Td26">S21</td><td rowspan="1" colspan="1" class="key" id="Td27">6</td><td id="Td28">S22</td></tr><tr><td id="Td29">S23</td><td id="Td30">S24</td><td id="Td31">S25</td><td id="Td32">S26</td><td id="Td33">S27</td><td id="Td34">S28</td><td id="Td35">S29</td></tr><tr><td id="Td36">S30</td><td rowspan="1" colspan="1" class="key" id="Td37">7</td><td id="Td38">S31</td><td rowspan="1" colspan="1" class="key" id="Td39">8</td><td id="Td40">S32</td><td rowspan="1" colspan="1" class="key" id="Td41">9</td><td id="Td42">S33</td></tr><tr><td id="Td43">S34</td><td id="Td44">S35</td><td id="Td45">S36</td><td id="Td46">S37</td><td id="Td47">S38</td><td id="Td48">S39</td><td id="Td49">S40</td></tr></table>
    <canvas id="touchCanvas" width="442" height="442"></canvas>
    </div-->
    
    <canvas id="touchCanvas" width="600" height="600"></canvas>
    <input id="btnSubmit" type=button value="Submit Gesture" onClick="submitGesture()"/>
    <p ID="console"></p>
    <img id="canvasBG" src="canvasBG.png" style="visibility:hidden"/>
</asp:Content>

<asp:Content ID="Scripts" runat="server" ContentPlaceHolderID="EndScripts">
    <script src="Scripts/jquery-1.4.1.js" type="text/javascript"></script>
<script src="Scripts/functions.js" type="text/javascript"></script>
<script type="text/javascript">
    function log(txt) {
        document.getElementById('console').innerHTML = txt + "<br />" + document.getElementById('console').innerHTML;
    }

    var canvas = document.getElementById('touchCanvas');
    var context = canvas.getContext('2d');
    context.clearRect(0, 0, canvas.width, canvas.height);
    context.drawImage(document.getElementById("canvasBG"), 0, 0, canvas.width, canvas.height);

    //document.getElementById('touchField').appendChild(createKeyTable(1,3));
    document.getElementById('touchCanvas').addEventListener('touchstart', onTouchEvent, true);
    document.getElementById('touchCanvas').addEventListener('touchmove', onTouchEvent, true);
    document.getElementById('touchCanvas').addEventListener('touchend', onTouchEvent, true);
    //enableTouch(true);

</script>
</asp:Content>