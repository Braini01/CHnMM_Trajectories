/*
function createKeyTable(sizeKey, nKeysPerRow) {
    var table = document.createElement("table");
    table.id = "keyTable";
    table.setAttribute("align", "center");

    var nRows = sizeKey * (2 * nKeysPerRow + 1);
    var nCols = nRows;
    var keyNumber = 1;
    var sensorNumber = 1;
    var tr, td, r, c;

    for (r = 0; r < nRows; r++) {
        tr = document.createElement("tr");
        for (c = 0; c < nCols; c++) {
            //is sensorrow or sensorcol?
            if ((Math.floor(r / sizeKey)) % 2 === 0 || (Math.floor(c / sizeKey)) % 2 === 0) {
                td = document.createElement("td");
                td.id = "S" + sensorNumber;
                td.innerHTML = "S" + sensorNumber++;
                tr.appendChild(td);
            }
            else {
                //key
                if (((r - sizeKey) % (sizeKey * 2) == 0) && ((c - sizeKey) % (sizeKey * 2) == 0)) {
                    td = document.createElement("td");
                    td.id = keyNumber;
                    td.innerHTML = keyNumber++;
                    td.className = "key";
                    td.setAttribute("colspan", sizeKey);
                    td.setAttribute("rowspan", sizeKey);
                    tr.appendChild(td);
                }
            }
        }
        table.appendChild(tr);
    }

    return table;
}
*/

function getDeviceInfo() {

    //collect device data
    var deviceData = new Object();
    deviceData.screenW = window.screen.width;
    deviceData.screenH = window.screen.height;
    deviceData.platform = navigator.platform;
    deviceData.userAgent = navigator.userAgent;

    //size of 
    return deviceData;
}

/*
function getTraceCount() {
    var gestureID = document.getElementById('MainContent_cbGestures').value;
    $.get('InputData.aspx', 'GetTraceCount=' + gestureID, function (data) {
        alert('Load was performed: ' + data);
    });
}
*/
function sendGestureData(userID, gestureID) {

    var deviceInfo = getDeviceInfo();
    var rect = touchCanvas.getBoundingClientRect();
    var tfInfo = new Object();
    tfInfo.top = Math.round(rect.top);
    tfInfo.bottom = Math.round(rect.bottom);
    tfInfo.left = Math.round(rect.left);
    tfInfo.right = Math.round(rect.right);
    tfInfo.width = Math.round(rect.width);
    tfInfo.height = Math.round(rect.height);
    var dataArray = [userID, gestureID, deviceInfo, tfInfo, gestureData];

    $.post("InputData.aspx", JSON.stringify(dataArray));
}

function clearCanvas(canvas, context) {
    context.clearRect(0, 0, canvas.width, canvas.height);
    context.drawImage(document.getElementById("canvasBG"), 0, 0, canvas.width, canvas.height);
}

function submitGesture() {
    if (gestureStarted || gestureData == null) return;
    var userID = document.getElementById('MainContent_cbUsers').value;
    var gestureID = document.getElementById('MainContent_cbGestures').value;
    sendGestureData(userID, gestureID);

    var canvas = document.getElementById('touchCanvas');
    var context = canvas.getContext('2d');
    clearCanvas(canvas, context);
    gestureData = null;

    //getTraceCount();
}

//global vars
var gestureData = null;
var touchCanvas = document.getElementById("touchCanvas");
var gestureStarted = false;
function onTouchEvent(e) {
    e.preventDefault();
    var time = new Date().getTime();
    if (e.type == "touchend") {
        if (e.touches.length == 0 && gestureData != null) {
            //touchData is empty and doesnt need to be added to gestureData
            //use timestamp somehow?
            //gesture end? (gestureData can be already null if two or more fingers leave the surface simultaneously)
            //var userID = document.getElementById('MainContent_cbUsers').value;
            //var gestureID = document.getElementById('MainContent_cbGestures').value;
            var strokes = createStrokes(gestureData);
            drawGestureVisualizationPoints(strokes);

            gestureStarted = false;

            //enableTouch(false);
            /*
            var r = confirm("Trace was ok?");
            if (r == true) {
                sendGestureData(userID, gestureID);
            }
            
            var canvas = document.getElementById('touchCanvas');
            var context = canvas.getContext('2d');
            clearCanvas(canvas, context);
            */
            //sendGestureData(userID, gestureID);
            //log(JSON.stringify(gestureData));
            //log("End of gesture");
            //gestureData = null;

            //enableTouch(true);
        }
    }
    else {
        var rect = touchCanvas.getBoundingClientRect();
        var touchFieldW = Math.round(rect.width);
        var touchFieldH = Math.round(rect.height);
        var nTouches = e.targetTouches.length;
        var touchData = new Array();
        for (var i = 0; i < nTouches; i++) {

            var touch = e.targetTouches[i];
            var dataObj = new Object();
            dataObj.id = touch.identifier;
            dataObj.radiusX = touch.radiusX;
            dataObj.radiusY = touch.radiusY;
            dataObj.rotationAngle = touch.rotationAngle;
            dataObj.force = touch.force;
            //dataObj.target = touch.target.id;
            //dataObj.element = document.elementFromPoint(touch.clientX, touch.clientY).id;
            dataObj.time = time;

            //calculate x and y relative to toucharea
            dataObj.x = (touch.clientX - Math.round(rect.left)) / touchFieldW;
            dataObj.y = (touch.clientY - Math.round(rect.top)) / touchFieldH;

            //ignore data outside surface or cancel gesture?

            touchData.push(dataObj);
            //log(JSON.stringify(touchData));
        }

        //log(e.type.toString());
        //log(JSON.stringify(touchData));
        //$.post("InputData.aspx", JSON.stringify(touchData));

        if (e.type == "touchstart") {
            //begin of new gesture?
            //if (gestureData == null) {
            if (!gestureStarted)
            {
                //log("Start of gesture");
                gestureData = new Array();
                gestureStarted = true;
            }
            gestureData.push(touchData);
        }
        else if (e.type == "touchmove") {
            gestureData.push(touchData);
        }
        else log("Unknown event type");
    }

    function createStrokes(gestureData) {
        var strokes = [];
        var nStrokes = 0;
        var IdToStrokeMap = []; //assoziativer array / dictionary

        for (var i = 0; i < gestureData.length; i++) {
            var touchEvent = gestureData[i];
            for (var j = 0; j < touchEvent.length; j++) {
                var touchData = touchEvent[j];

                var strokeID = touchData.id;
                var strokeTouchData = new Object;
                strokeTouchData.x = touchData.x;
                strokeTouchData.y = touchData.y;

                //add data to stroke
                //is stroke known?
                if (IdToStrokeMap[strokeID] == null) {
                    IdToStrokeMap[strokeID] = [strokeTouchData];
                }
                else IdToStrokeMap[strokeID].push(strokeTouchData);
            }
        }

        for (var id in IdToStrokeMap)
            strokes.push(IdToStrokeMap[id]);

        return strokes;
    }

    
    /*
    function enableTouch(enable) {
        if (enable) {
            document.getElementById('touchCanvas').addEventListener('touchstart', onTouchEvent, true);
            document.getElementById('touchCanvas').addEventListener('touchmove', onTouchEvent, true);
            document.getElementById('touchCanvas').addEventListener('touchend', onTouchEvent, true);
        }
        else {
            document.getElementById('touchCanvas').removeEventListener('touchstart', onTouchEvent, true);
            document.getElementById('touchCanvas').removeEventListener('touchmove', onTouchEvent, true);
            document.getElementById('touchCanvas').removeEventListener('touchend', onTouchEvent, true);
        }
    }
    */
    function drawGestureVisualization(strokes) {
        var canvas = document.getElementById('touchCanvas');
        var context = canvas.getContext('2d');
        //context.clearRect(0, 0, canvas.width, canvas.height);
        clearCanvas(canvas, context);
        context.lineCap = 'round';
        context.lineWidth = 10;
        context.strokeStyle = '#ff0000';

        for (var stroke in strokes) {
            context.beginPath();
            for (var j = 0; j < strokes[stroke].length; j++) {

                if (j == 0) context.moveTo(strokes[stroke][j].x * canvas.width, strokes[stroke][j].y * canvas.height);
                else context.lineTo(strokes[stroke][j].x * canvas.width, strokes[stroke][j].y * canvas.height);

            }
            context.stroke();
        }
    }

    function drawGestureVisualizationPoints(strokes) {
        var canvas = document.getElementById('touchCanvas');
        var context = canvas.getContext('2d');
        //context.clearRect(0, 0, canvas.width, canvas.height);
        clearCanvas(canvas, context);
        context.lineCap = 'round';
        context.lineWidth = 2;
        context.strokeStyle = 'black';
        context.fillStyle = 'red';

        var colors = ["red", "green", "yellow", "blue", "orange", "violet", "brown"];
        var strokeIndex = 0;
        for (var stroke in strokes) {
            context.fillStyle = colors[strokeIndex++ % colors.length];
            for (var j = 0; j < strokes[stroke].length; j++) {
                context.beginPath();
                context.arc(strokes[stroke][j].x * canvas.width, strokes[stroke][j].y * canvas.height, 10, 0, 2 * Math.PI, false);
                context.fill();
                context.stroke();
                context.closePath();
            }
        }
    }
}

/*
    <!-- 
    <asp:Label ID="lblUsername" runat="server" Text="Username: "></asp:Label>
    <asp:DropDownList ID="cbUser" runat="server" DataSourceID="Users" DataTextField="Username" DataValueField="Id"></asp:DropDownList>

    
    <asp:EntityDataSource ID="Users" runat="server" 
        ConnectionString="name=dbEntities" DefaultContainerName="dbEntities" 
        EnableFlattening="False" EntitySetName="Users" EntityTypeFilter="User" 
        Select="it.[Username], it.[Id]"></asp:EntityDataSource>
    
    <asp:Label ID="lblGesture" runat="server" Text="Gesture: "></asp:Label>
    <asp:DropDownList ID="cbGesture" runat="server" DataSourceID="Users" 
        DataTextField="Gestures" DataValueField="Gestures">
    </asp:DropDownList>
    -->*/