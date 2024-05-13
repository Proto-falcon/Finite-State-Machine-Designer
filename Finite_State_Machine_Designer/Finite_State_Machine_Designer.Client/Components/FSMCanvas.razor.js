const STATETEXTNEWLINE = 20;
const CANVASTEXTFONTSTYLE = '20px "Times New Roman", serif';
const FINALSTATECIRCLERATIO = 0.8;

const CANVASTEXTVERTICAL = Object.freeze(
    {
        Centre: 0,
        Down: 1,
        Up: 2
    }
);

class CanvasCoordinate {
    x = 0;
    y = 0;

    /**
     * 
     * @param {number} x
     * @param {number} y
     */
    constructor(x, y) {
        this.x = x;
        this.y = y;
    }
}


class FiniteState {
    coordinate = new CanvasCoordinate();
    radius = 1;
    isFinalState = false;
    text = "";
    isDrawable = false;

    /**
     * 
     * @param {CanvasCoordinate} coordinate
     * @param {number} radius
     */
    constructor(coordinate, radius) {
        this.coordinate = coordinate;
        this.radius = radius;
    }
}


class StateTransition {
    fromState = new FiniteState();
    fromCoord = new CanvasCoordinate();
    fromAngle = 0;
    toState = new FiniteState();
    toCoord = new CanvasCoordinate();
    toAngle = 0;
    angle = 0;
    centerArc = new CanvasCoordinate();
    radius = 0;
    isCurved = false;
    text = "";
    anchor = new CanvasCoordinate();
    isReversed = false;
}

class FiniteStateMachine {
    finalStates = [new FiniteState()];
    initialStates = [new FiniteState()];
    states = [new FiniteState()];
    transitionSearchRadius = 0;
    transitions = [new StateTransition()];
}

/** @type {?CanvasRenderingContext2D}*/
let canvasCtx;

/** @type {HTMLCanvasElement}*/
let canvasElement;

/**
 * Gets canvas context
 * @param {string} id HTML Id of Canvas
 * @returns {boolean} true when the retrieveing the canvas context was successfull, otherwise false.
 */
export function getCanvasContext(id) {
    canvasElement = document.getElementById(id);
    /** @type {?CanvasRenderingContext2D}*/
    let canvasContext;

    if (canvasElement !== null && canvasElement.getContext) {
        canvasContext = canvasElement.getContext("2d");
        canvasContext.font = CANVASTEXTFONTSTYLE;
    }
    canvasCtx = canvasContext;
    return canvasCtx !== null && canvasCtx !== undefined;
}

/**
 * Checks whether the canvas element and context are null or undefined.
 * @param {CanvasRenderingContext2D} drawingCtx A 2d canvas rendering context
 * @returns {boolean} true when both aren't null or undefined, false when either are
 * null or undefined.
 */
function checkCanvas(drawingCtx) {
    if (
        (drawingCtx !== undefined && drawingCtx.canvas !== undefined)
        && (drawingCtx !== null && drawingCtx.canvas !== null)
    ) {
        return true;
    }
    return false;
}

/**
 * Clears the whole canvas
 * @returns {boolean} True if cleared the whole canvas successfully,
 * otherwise there wasn't canvas (context) to doesn't exist.
 */
export function clearCanvas() {
    if (checkCanvas(canvasCtx)) {
        canvasCtx.clearRect(
            0,
            0,
            canvasCtx.canvas.width,
            canvasCtx.canvas.height
        );
        return true;
    }
    return false;
}

/**
 * Creates a state at a position within the canvas with colour.
 * @param {FiniteState} state A state in Finite State Machine.
 * @param {string} colour Colour when drawn
 * @param {boolean} editable Flag to show vertical bar appear and reappear in popular text editors.
 * @param {CanvasRenderingContext2D} drawingCtx A 2d canvas rendering context
 * @returns {boolean} True when created successfully, otherwise can't create it because canvas (context) doesn't exist.
 */
export function drawState(state, colour, editable, drawingCtx) {
    if (drawingCtx === undefined || drawingCtx === null) {
        drawingCtx = canvasCtx;
    }
    if (checkCanvas(drawingCtx)) {
        drawingCtx.beginPath();
        drawingCtx.strokeStyle = colour;
        let stateCoord = state.coordinate;
        drawingCtx.arc(stateCoord.x, stateCoord.y, state.radius, 0, 2 * Math.PI);
        drawingCtx.closePath();
        drawingCtx.stroke();

        if (state.isFinalState) {
            drawingCtx.beginPath();
            drawingCtx.arc(stateCoord.x, stateCoord.y, state.radius * FINALSTATECIRCLERATIO, 0, 2 * Math.PI);
            drawingCtx.closePath();
            drawingCtx.stroke();
        }

        drawCanvasText(drawingCtx, stateCoord.x, stateCoord.y, colour, state.text.split("\n"), editable);

        return true;
    }
    return false;
}

/**
 * Draws a transition on the canvas.
 * @param {StateTransition} transition Transition between 2 states
 * @param {string} colour Colour of the transition
 * @param {boolean} editable Flag to tell that there should be caret appear
 * @param {CanvasRenderingContext2D} drawingCtx A 2d canvas rendering context
 * @returns {boolean} True when created successfully, otherwise can't create it because canvas (context) doesn't exist.
 */
export function drawTransition(transition, colour, editable, drawingCtx) {
    if (drawingCtx === undefined || drawingCtx === null) {
        drawingCtx = canvasCtx;
    }
    if (checkCanvas(drawingCtx)) {
        let arrowCoord = new CanvasCoordinate();
        let arrowAngle = transition.angle;
        let textX = 0;
        let textY = 0;
        let textAngle = 0;
        let textLines = transition.text.split("\n");
        let reverseScale = transition.isReversed ? -1 : 1;
        let longestLine = "";
        textLines.forEach(x => longestLine = x.length > longestLine.length ? x : longestLine);
        let vertialAlignment = CANVASTEXTVERTICAL.Centre;

        drawingCtx.strokeStyle = colour;
        drawingCtx.beginPath();
        if (!transition.isCurved) {
            if (!transition.fromState.isDrawable) {
                textX = transition.fromCoord.x - ((drawingCtx.measureText(longestLine).width / 2 + 20) * Math.cos(transition.angle));
                textY = transition.fromCoord.y - ((STATETEXTNEWLINE/2) * textLines.length * Math.sin(transition.angle));
            }
            else {
                let fromCoord = transition.fromCoord;
                let toCoord = transition.toCoord;

                textX = ((toCoord.x + fromCoord.x) / 2);
                textY = ((toCoord.y + fromCoord.y) / 2);
                /**
                 * Gives the angle between the transition and positive y-axis
                 * (positive y is down when dealing with position in webpages)
                 */
                let invertedAngle = Math.atan2(toCoord.x - fromCoord.x, toCoord.y - fromCoord.y);
                textAngle = (transition.isReversed * Math.PI) - invertedAngle;
            }

            drawingCtx.moveTo(transition.fromCoord.x, transition.fromCoord.y);
            drawingCtx.lineTo(transition.toCoord.x, transition.toCoord.y);
            arrowCoord = new CanvasCoordinate(transition.toCoord.x, transition.toCoord.y);
        }
        else {
            let centreCoord = transition.centerArc;
            drawingCtx.arc(centreCoord.x, centreCoord.y, transition.radius, transition.fromAngle, transition.toAngle, transition.isReversed);
            arrowAngle = transition.toAngle;
            arrowCoord.x = centreCoord.x + (Math.cos(arrowAngle) * transition.radius);
            arrowCoord.y = centreCoord.y + (Math.sin(arrowAngle) * transition.radius);
            arrowAngle += reverseScale * (Math.PI / 2);

            let startAngle = transition.fromAngle;
            let endAngle = transition.toAngle;
            endAngle += (endAngle < startAngle) ? (2 * Math.PI) : 0;

            textAngle = (((endAngle + startAngle) / 2)) + (transition.isReversed * Math.PI);
            let cos = Math.cos(textAngle);
            textX = transition.centerArc.x + (cos * (transition.radius + 5));
            textY = transition.centerArc.y + (Math.sin(textAngle) * (transition.radius + 5));
        }
        drawingCtx.stroke();
        drawingCtx.closePath();
        drawArrow(arrowCoord.x, arrowCoord.y, arrowAngle, colour);

        if (transition.fromState.isDrawable) {
            let cos = Math.cos(textAngle);
            let sin = Math.sin(textAngle);
            let cornerPointX = (drawingCtx.measureText(longestLine).width / 2 + 5) * (cos > 0 ? 1 : -1);
            let cornerPointY = 20 * (sin > 0 ? 1 : -1);
            let slide = (Math.pow(sin, 41) * cornerPointX)
                - (Math.pow(cos, 11) * cornerPointY);
            textX += cornerPointX - sin * slide;
            textY += cornerPointY + (cos * slide) - 0;

            if (Math.abs(transition.angle) < (Math.PI / 2 - 0.3) || Math.abs(transition.angle) > (Math.PI / 2 + 0.3)) {
                if (textAngle > 2 * Math.PI) {
                    textAngle = textAngle - Math.PI;
                }
                else if (textAngle > Math.PI) {
                    textAngle = Math.PI - textAngle;
                }
                if (textAngle > 0) {
                    vertialAlignment = CANVASTEXTVERTICAL.Down;
                }
                else {
                    vertialAlignment = CANVASTEXTVERTICAL.Up;
                }
            }
        }

        drawCanvasText(drawingCtx, textX, textY, colour, textLines, editable, vertialAlignment);
        return true;
    }
    return false;
}

/**
 * Draws Canvas Text
 * 
 * @param {number} x
 * @param {number} y
 * @param {string} colour
 * @param {string[]} textLines
 * @param {boolean} editable
 * @param {number} [vertialAlignment=0] 
 * @param {CanvasRenderingContext2D} drawingCtx A 2d canvas rendering context
 */
function drawCanvasText(drawingCtx, x, y, colour, textLines, editable, vertialAlignment = 0) {
    if (drawingCtx === undefined || drawingCtx === null) {
        drawingCtx = canvasCtx;
    }
    if (checkCanvas(drawingCtx)) {
        let caretX = 0;
        let caretY = 0;
        let textX = 0;
        let textY = 0;

        if (textLines.length <= 0) {
            caretX = x + 0.5;
            caretY = y + 10;
        }
        else {
            let initialY = 0;
            if (vertialAlignment == CANVASTEXTVERTICAL.Centre) {
                initialY = y - ((textLines.length - 1) * (STATETEXTNEWLINE / 2));
            }
            else if (vertialAlignment == CANVASTEXTVERTICAL.Up) {
                initialY = y - ((textLines.length - 1) * STATETEXTNEWLINE);
            }
            else {
                initialY = y;
            }
            textY = initialY + 10;
            drawingCtx.fillStyle = colour;

            for (var i = 0; i < textLines.length; i++) {
                let textMetric = drawingCtx.measureText(textLines[i]);

                let halfWidth = Math.round(textMetric.width / 2);

                textX = x - halfWidth;
                caretX = x + halfWidth;
                drawingCtx.fillText(textLines[i], textX, textY);

                caretY = textY
                textY += STATETEXTNEWLINE
            }
        }

        if (editable) {
            drawCaret(caretX, caretY, textLines.length > 0, drawingCtx);
        }
    }
}

/**
 * Draw line used in text editors
 * @param {number} x
 * @param {number} y
 * @param {boolean} hasOffset
 * @param {CanvasRenderingContext2D} drawingCtx A 2d canvas rendering context
 */
export function drawCaret(x, y, hasOffset, drawingCtx) {
    if (drawingCtx === undefined || drawingCtx === null) {
        drawingCtx = canvasCtx;
    }
    if (checkCanvas(drawingCtx)) {
        drawingCtx.beginPath();

        if (hasOffset) {
            x += 1.5;
        }

        drawingCtx.moveTo(x, y - 20.5);
        drawingCtx.lineTo(x, y + 5.5);
        drawingCtx.closePath();
        drawingCtx.stroke();
    }
}


/**
 * Draws an arrow
 * @param {number} x X co-ordinate of the tip of the arrow
 * @param {number} y Y co-ordinate of the tip of the arrow
 * @param {number} angle Angle of the arrow
 * @param {string} colour Colour of the arrow
 * @param {CanvasRenderingContext2D} drawingCtx A 2d canvas rendering context
 */
function drawArrow(x, y, angle, colour, drawingCtx) {
    if (drawingCtx === undefined || drawingCtx === null) {
        drawingCtx = canvasCtx;
    }
    if (checkCanvas(drawingCtx)) {
        var cosAngle = Math.cos(angle);
        var sineAngle = Math.sin(angle);

        drawingCtx.fillStyle = colour;
        drawingCtx.beginPath();
        drawingCtx.moveTo(x, y);
        drawingCtx.lineTo(x - 8 * cosAngle + 5 * sineAngle, y - 8 * sineAngle - 5 * cosAngle);
        drawingCtx.lineTo(x - 8 * cosAngle - 5 * sineAngle, y - 8 * sineAngle + 5 * cosAngle);
        drawingCtx.closePath();
        drawingCtx.fill();
    }
}

/**
 * Saves the finite state machine to local storage
 * @param {FiniteStateMachine} fsm 
 */
export function saveFSM(fsm) {
    localStorage["fsm"] = JSON.stringify(fsm);
}

export function loadFSM() {
    /**@type {string} */
    let fsmJSONText = localStorage["fsm"];
    if (fsmJSONText === undefined) {
        return null;
    };
    return JSON.parse(fsmJSONText);
}

/**
 * Downloads a png of the Finite State Machine
 * @param {FiniteStateMachine} fsm
 * @param {string} colour 
 */
export function SaveAsPNG(fsm, colour) {
    /** @type {HTMLCanvasElement} */
    let tmpCanvas = document.createElement("canvas");
    tmpCanvas.width = canvasElement.width;
    tmpCanvas.height = canvasElement.height;
    let tmpCanvasCtx = tmpCanvas.getContext("2d");
    tmpCanvasCtx.font = CANVASTEXTFONTSTYLE;

    fsm.states.forEach(state =>
        drawState(state, colour, false, tmpCanvasCtx)
    );
    fsm.transitions.forEach(transition =>
        drawTransition(transition, colour, false, tmpCanvasCtx)
    );

    let pngData = tmpCanvas.toDataURL('image/png');
    let anchor = document.createElement('a')
    anchor.href = pngData;
    anchor.download = "Finite State Machine";
    anchor.click();
}

/**
 * Downloads a json file of Finite State Machine
 * @param {FiniteStateMachine} fsm
 */
export function saveAsJson(fsm) {
    let fsmJson = new Blob([JSON.stringify(fsm, null, 4)], {type: "application/json"});
    downloadFile(fsmJson, "Finite State Machine");
}

/**
 * Downloads a blob object with a filename
 * @param {Blob} blob
 * @param {string} fileName
 */
function downloadFile(blob, fileName) {
    let fileUrl = URL.createObjectURL(blob);
    let anchor = document.createElement('a')
    anchor.href = fileUrl;
    anchor.download = fileName;
    anchor.click();
    URL.revokeObjectURL(fileUrl);
}
