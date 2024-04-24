const CANVASWIDTHRATIO = 0.8;
const CANVASHEIGHTRATIO = 0.8;
const STATETEXTNEWLINE = 20;
const CANVASTEXTFONTSTYLE = '20px "Times New Roman", serif';
const FINALSTATECIRCLERATIO = 0.8;


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


/** @type {?CanvasRenderingContext2D}*/
let canvasCtx;

/**
 * Gets canvas context
 * @param {string} id HTML Id of Canvas
 * @returns {boolean} true when the retrieveing the canvas context was successfull, otherwise false.
 */
export function getCanvasContext(id) {
    /** @type {HTMLCanvasElement}*/
    let canvasElement = document.getElementById(id);

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
 * @returns {boolean} true when both aren't null or undefined, false when either are
 * null or undefined.
 */
function checkCanvas() {
    if (
        (canvasCtx !== undefined && canvasCtx.canvas !== undefined)
        && (canvasCtx !== null && canvasCtx.canvas !== null)
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
    if (checkCanvas()) {
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
 * Updates the canvas dimensions when the window size changes
 * @param {Event} event
 */
function updateCanvasDimensions(event) {
    if (checkCanvas()) {
        canvasCtx.canvas.height = window.outerHeight * CANVASHEIGHTRATIO;
        canvasCtx.canvas.width = window.outerWidth * CANVASWIDTHRATIO;

        canvasCtx.font = CANVASTEXTFONTSTYLE;
    }
}

/**
 * Creates a state at a position within the canvas with colour.
 * @param {FiniteState} state A state in Finite State Machine.
 * @param {string} colour Colour when drawn
 * @param {boolean} editable Flag to show vertical bar appear and reappear in popular text editors.
 * @returns {boolean} True when created successfully, otherwise can't create it because canvas (context) doesn't exist.
 */
export function drawState(state, colour, editable) {
    if (checkCanvas()) {
        canvasCtx.beginPath();
        canvasCtx.strokeStyle = colour;
        let stateCoord = state.coordinate;
        canvasCtx.arc(stateCoord.x, stateCoord.y, state.radius, 0, 2 * Math.PI);
        //canvasCtx.arc(x, y, radius, 0, 2 * Math.PI);
        canvasCtx.closePath();
        canvasCtx.stroke();

        if (state.isFinalState) {
            canvasCtx.beginPath();
            canvasCtx.arc(stateCoord.x, stateCoord.y, state.radius * FINALSTATECIRCLERATIO, 0, 2 * Math.PI);
            canvasCtx.closePath();
            canvasCtx.stroke();
        }

        drawCanvasText(stateCoord.x, stateCoord.y, colour, state.text.split("\n"), editable);

        return true;
    }
    return false;
}

/**
 * Draws a transition on the canvas.
 * @param {StateTransition} transition Transition between 2 states
 * @param {string} colour Colour of the transition
 * @param {boolean} editable Flag to tell that there should be caret appear
 * @returns {boolean} True when created successfully, otherwise can't create it because canvas (context) doesn't exist.
 */
export function drawTransition(transition, colour, editable) {
    if (checkCanvas()) {
        let arrowCoord = new CanvasCoordinate();
        let arrowAngle = transition.angle;

        canvasCtx.strokeStyle = colour;
        canvasCtx.beginPath();
        if (!transition.isCurved) {
            canvasCtx.moveTo(transition.fromCoord.x, transition.fromCoord.y);
            canvasCtx.lineTo(transition.toCoord.x, transition.toCoord.y);
            arrowCoord = new CanvasCoordinate(transition.toCoord.x, transition.toCoord.y);
        }
        else {
            let centreCoord = transition.centerArc;
            canvasCtx.arc(centreCoord.x, centreCoord.y, transition.radius, transition.fromAngle, transition.toAngle, transition.isReversed);
            arrowAngle = transition.toAngle;
            arrowCoord.x = centreCoord.x + (Math.cos(arrowAngle) * transition.radius);
            arrowCoord.y = centreCoord.y + (Math.sin(arrowAngle) * transition.radius);
            arrowAngle += (transition.isReversed ? -1 : 1) * (Math.PI / 2);
        }
        canvasCtx.stroke();
        canvasCtx.closePath();
        drawArrow(arrowCoord.x, arrowCoord.y, arrowAngle, colour);
        return true;
    }
    return false;
}

function drawCanvasText(x, y, colour, textLines, editable) {
    let caretX = 0;
    let caretY = 0;
    let textX = 0;
    let textY = 0;


    if (textLines.length <= 0) {
        caretX = x + 0.5;
        caretY = y + 10;
    }
    else {
        let initialY = y - ((textLines.length - 1) * (STATETEXTNEWLINE / 2));
        textY = initialY + 10;
        canvasCtx.fillStyle = colour;

        for (var i = 0; i < textLines.length; i++) {
            let textMetric = canvasCtx.measureText(textLines[i]);

            let halfWidth = Math.round(textMetric.width / 2);

            textX = x - halfWidth;
            canvasCtx.fillText(textLines[i], textX, textY);

            caretX = x + halfWidth;
            caretY = textY
            textY += STATETEXTNEWLINE
        }
    }

    if (editable) {
        drawTextLine(caretX, caretY, textLines.length > 0);
    }
}

/**
 * Draw line used in text editors
 * @param {number} x
 * @param {number} y
 * @param {boolean} hasOffset
 */
export function drawTextLine(x, y, hasOffset) {
    canvasCtx.beginPath();

    if (hasOffset) {
        x += 1.5;
    }

    canvasCtx.moveTo(x, y - 20.5);
    canvasCtx.lineTo(x, y + 5.5);
    canvasCtx.closePath();
    canvasCtx.stroke();
}


/**
 * Draws an arrow
 * @param {number} x X co-ordinate of the tip of the arrow
 * @param {number} y Y co-ordinate of the tip of the arrow
 * @param {number} angle Angle of the arrow
 * @param {string} colour Colour of the arrow
 */
function drawArrow(x, y, angle, colour) {
    var cosAngle = Math.cos(angle);
    var sineAngle = Math.sin(angle);

    canvasCtx.fillStyle = colour;
    canvasCtx.beginPath();
    canvasCtx.moveTo(x, y);
    canvasCtx.lineTo(x - 8 * cosAngle + 5 * sineAngle, y - 8 * sineAngle - 5 * cosAngle);
    canvasCtx.lineTo(x - 8 * cosAngle - 5 * sineAngle, y - 8 * sineAngle + 5 * cosAngle);
    canvasCtx.closePath();
    canvasCtx.fill();
}

addEventListener("resize", updateCanvasDimensions);
//addEventListener("visibilitychange");
