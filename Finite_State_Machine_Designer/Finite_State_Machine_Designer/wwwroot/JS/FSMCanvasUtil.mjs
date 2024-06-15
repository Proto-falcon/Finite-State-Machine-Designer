export const FONTSTYLE = "Times New Roman, serif";
export const FONTSIZE = 20;
export const CANVASTEXTFONTSTYLE = `${FONTSIZE}px ${FONTSTYLE}`;
export const FINALSTATECIRCLERATIO = 0.8;
export const ARROWWIDTH = 8;
export const ARROWHEIGHT = 5;

export const CANVASTEXTVERTICAL = Object.freeze(
    {
        Centre: 0,
        Down: 1,
        Up: 2
    }
);

/**
 * @typedef {{
 *      x: number,
 *      y: number,
 * }} CanvasCoordinate
 */
export class CanvasCoordinate {
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

/**
 * @typedef {{
 *      id: string,
 *      coordinate: CanvasCoordinate,
 *      radius: number,
 *      isFinalState: boolean,
 *      text: string,
 *      isDrawable: boolean,
 * }} FiniteState
 */
export class FiniteState {
    id = "";
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

/**
 * @typedef {{
 *      id: string,
 *      fromState: FiniteState,
 *      fromCoord: CanvasCoordinate,
 *      fromAngle: number,
 *      toState: FiniteState,
 *      toCoord: CanvasCoordinate,
 *      toAngle: number,
 *      angle: number,
 *      centerArc: CanvasCoordinate,
 *      radius: number,
 *      isCurved: boolean,
 *      text: string,
 *      anchor: CanvasCoordinate,
 *      parallelAxis: number,
 *      perpendicularAxis: number,
 *      isReversed: boolean,
 * }} Transition
 */
export class Transition {
    id = "";
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
    parallelAxis = 0;
    perpendicularAxis = 0;
    isReversed = false;
}

/**
 * @typedef {{
 *      id: string,
 *      name: string,
 *      description: string,
 *      width: number,
 *      height: number,
 *      finalStates: Array<FiniteState>,
 *      initialStates: Array<FiniteState>,
 *      states: Array<FiniteState>,
 *      transitionSearchRadius: number,
 *      transitions: Array<Transition>,
 * }} FiniteStateMachine
 */
export class FiniteStateMachine {
    id = "";
    name = "";
    description = "";
    width = 0;
    height = 0;
    finalStates = [new FiniteState()];
    initialStates = [new FiniteState()];
    states = [new FiniteState()];
    transitionSearchRadius = 0;
    transitions = [new Transition()];
}

/** @type {?CanvasRenderingContext2D}*/
export let canvasCtx;

/** @type {HTMLCanvasElement}*/
export let canvasElement;

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
export function checkCanvas(drawingCtx) {
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
 * Draws backgroundColour of canvas
 * @param {CanvasRenderingContext2D} drawingCtx Canvas 2d rendering context
 * @param {string} colour
 */
export function drawBackgroundColour(colour, drawingCtx) {
    if (drawingCtx === undefined || drawingCtx === null) {
        drawingCtx = canvasCtx;
    }
    drawingCtx.fillStyle = colour;
    drawingCtx.fillRect(0, 0, drawingCtx.canvas.width, drawingCtx.canvas.height);
}

/**
 * Creates a state at a position within the canvas with colour.
 * @param {FiniteState} state A state in Finite State Machine.
 * @param {string} colour Colour when drawn
 * @param {boolean} editable Flag to show vertical bar appear and reappear in popular text editors.
 * @param {CanvasRenderingContext2D} drawingCtx A 2d canvas rendering context
 * @param {number} [scale=1] Scale that the state will be drawn in.
 * @returns {boolean} True when created successfully, otherwise can't create it because canvas (context) doesn't exist.
 */
export function drawState(state, colour, editable, drawingCtx, scale = 1) {
    if (drawingCtx === undefined || drawingCtx === null) {
        drawingCtx = canvasCtx;
    }
    if (checkCanvas(drawingCtx)) {
        drawingCtx.beginPath();
        drawingCtx.strokeStyle = colour;
        drawingCtx.lineWidth = scale;
        let stateCoord = { x: state.coordinate.x * scale, y: state.coordinate.y * scale };
        let stateRadius = state.radius * scale;
        drawingCtx.arc(stateCoord.x, stateCoord.y, stateRadius, 0, 2 * Math.PI);
        drawingCtx.closePath();
        drawingCtx.stroke();

        if (state.isFinalState) {
            drawingCtx.beginPath();
            drawingCtx.arc(stateCoord.x, stateCoord.y, stateRadius * FINALSTATECIRCLERATIO, 0, 2 * Math.PI);
            drawingCtx.closePath();
            drawingCtx.stroke();
        }

        drawCanvasText(drawingCtx, state.coordinate.x, state.coordinate.y, colour,
            state.text.split("\n"), editable, CANVASTEXTVERTICAL.Centre, scale);

        return true;
    }
    return false;
}

/**
 * Draws a transition on the canvas.
 * @param {Transition} transition Transition between 2 states
 * @param {string} colour Colour of the transition
 * @param {boolean} editable Flag to tell that there should be caret appear
 * @param {CanvasRenderingContext2D} drawingCtx A 2d canvas rendering context
 * @param {number} [scale=1] Scale that the state will be drawn in.
 * @returns {boolean} True when created successfully, otherwise can't create it because canvas (context) doesn't exist.
 */
export function drawTransition(transition, colour, editable, drawingCtx, scale = 1) {
    if (drawingCtx === undefined || drawingCtx === null) {
        drawingCtx = canvasCtx;
    }
    if (checkCanvas(drawingCtx)) {
        let arrowCoord = new CanvasCoordinate();
        let arrowAngle = transition.angle;

        drawingCtx.strokeStyle = colour;
        drawingCtx.lineWidth = scale;
        drawingCtx.beginPath();
        if (!transition.isCurved) {
            let fromCoord = { x: transition.fromCoord.x * scale, y: transition.fromCoord.y * scale };
            let toCoord = { x: transition.toCoord.x * scale, y: transition.toCoord.y * scale };
            drawingCtx.moveTo(fromCoord.x, fromCoord.y);
            drawingCtx.lineTo(toCoord.x, toCoord.y);
            arrowCoord = transition.toCoord;
        }
        else {
            let centreCoord = transition.centerArc;
            drawingCtx.arc(centreCoord.x * scale, centreCoord.y * scale, transition.radius * scale,
                transition.fromAngle, transition.toAngle, transition.isReversed);
            arrowAngle = transition.toAngle;
            arrowCoord = transition.toCoord;
            arrowAngle += (transition.isReversed ? -1 : 1) * (Math.PI / 2);
        }
        drawingCtx.stroke();
        drawingCtx.closePath();
        drawArrow(arrowCoord.x * scale, arrowCoord.y * scale, arrowAngle,
            colour, ARROWWIDTH * scale, ARROWHEIGHT * scale, drawingCtx);

        let textCoordInfo = SetTransitionTextCoords(transition, drawingCtx);
        let textX = textCoordInfo.textX;
        let textY = textCoordInfo.textY;
        let vertialAlignment = textCoordInfo.vertialAlignment;

        drawCanvasText(drawingCtx, textX, textY, colour, transition.text.split("\n"), editable, vertialAlignment, scale);
        return true;
    }
    return false;
}

/**
 * Coordinates and angle of text for transition.
 * @param {Transition} transition
 * @param {CanvasRenderingContext2D} drawingCtx 2D canvas drawing context
 * @param {number} [scale=1] Scale that the state will be drawn in.
 * @returns {{textX: number, textY: number, textAngle: number, vertialAlignment: number}}
 * First two numbers are coordinates, the thrid is the text angle.
 * The last one is the vertical alignment of text.
 */
export function SetTransitionTextCoords(transition, drawingCtx, scale = 1) {
    let textX = 0;
    let textY = 0;
    let textAngle = 0;
    let vertialAlignment = CANVASTEXTVERTICAL.Centre;
    let textLines = transition.text.split("\n");
    let longestLine = "";
    textLines.forEach(x => longestLine = x.length > longestLine.length ? x : longestLine);
    let halfLongestLineWidth = drawingCtx.measureText(longestLine).width / 2;

    if (!transition.fromState.isDrawable) {
        let absLineWidth = (halfLongestLineWidth + 20);
        textX = (transition.fromCoord.x - (absLineWidth * Math.cos(transition.angle))) * scale;
        textY = (transition.fromCoord.y - ((FONTSIZE / 2) * textLines.length * Math.sin(transition.angle))) * scale;
    }
    else {
        if (!transition.isCurved) {
            let fromCoord = transition.fromCoord;
            let toCoord = transition.toCoord;

            textX = ((toCoord.x + fromCoord.x) / 2) * scale;
            textY = ((toCoord.y + fromCoord.y) / 2) * scale;
            /**
             * Gives the angle between the transition and positive y-axis
             * (positive y is down when dealing with position in webpages)
             */
            let invertedAngle = Math.atan2(toCoord.x - fromCoord.x, toCoord.y - fromCoord.y);
            textAngle = (transition.isReversed * Math.PI) - invertedAngle;
        }
        else {
            let startAngle = transition.fromAngle;
            let endAngle = transition.toAngle;
            endAngle += (endAngle < startAngle) ? (2 * Math.PI) : 0;

            textAngle = (((endAngle + startAngle) / 2)) + (transition.isReversed * Math.PI);
            let cos = Math.cos(textAngle);
            textX = (transition.centerArc.x + (cos * (transition.radius + 5))) * scale;
            textY = (transition.centerArc.y + (Math.sin(textAngle) * (transition.radius + 5))) * scale;
        }

        let cos = Math.cos(textAngle);
        let sin = Math.sin(textAngle);
        let cornerPointX = (halfLongestLineWidth + 5) * (cos > 0 ? 1 : -1) * scale;
        let cornerPointY = 20 * (sin > 0 ? 1 : -1) * scale;
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
    return { textX, textY, textAngle, vertialAlignment };
}

/**
 * Draws Canvas Text
 * 
 * @param {CanvasRenderingContext2D} drawingCtx A 2d canvas rendering context
 * @param {number} x
 * @param {number} y
 * @param {string} colour
 * @param {string[]} textLines
 * @param {boolean} editable
 * @param {number} [vertialAlignment=0] 
 * @param {number} [scale=1] Scale the canvas by a factor
 */
export function drawCanvasText(drawingCtx, x, y, colour, textLines, editable, vertialAlignment = 0, scale = 1) {
    if (drawingCtx === undefined || drawingCtx === null) {
        drawingCtx = canvasCtx;
    }
    if (checkCanvas(drawingCtx)) {
        let caretX = 0;
        let caretY = 0;
        let textX = 0;
        let textY = 0;

        if (textLines.length <= 0) {
            caretX = (x + 0.5) * scale;
            caretY = (y + (FONTSIZE / 2)) * scale;
        }
        else {
            let initialY = 0;
            if (vertialAlignment === CANVASTEXTVERTICAL.Centre) {
                initialY = y - ((textLines.length - 1) * (FONTSIZE / 2));
            }
            else if (vertialAlignment === CANVASTEXTVERTICAL.Up) {
                initialY = y - ((textLines.length - 1) * FONTSIZE);
            }
            else {
                initialY = y;
            }
            textY = (initialY + (FONTSIZE / 2)) * scale;
            drawingCtx.fillStyle = colour;

            for (var i = 0; i < textLines.length; i++) {
                let textMetric = drawingCtx.measureText(textLines[i]);

                let halfWidth = Math.round(textMetric.width / 2);

                textX = (x - halfWidth) * scale;
                caretX = (x + halfWidth) * scale;
                drawingCtx.fillText(textLines[i], textX, textY);

                caretY = textY * scale
                textY += FONTSIZE * scale
            }
        }

        if (editable) {
            drawCaret(caretX, caretY, textLines.length > 0, drawingCtx);
        }
    }
}

/**
 * Draws an arrow
 * @param {number} x X co-ordinate of the tip of the arrow
 * @param {number} y Y co-ordinate of the tip of the arrow
 * @param {number} angle Angle of the arrow
 * @param {string} colour Colour of the arrow
 * @param {number} width Width of the arrow
 * @param {number} height Height of the arrow
 * @param {CanvasRenderingContext2D} drawingCtx A 2d canvas rendering context
 */
export function drawArrow(x, y, angle, colour, width, height, drawingCtx) {
    if (drawingCtx === undefined || drawingCtx === null) {
        drawingCtx = canvasCtx;
    }
    if (checkCanvas(drawingCtx)) {
        let cosAngle = Math.cos(angle);
        let sineAngle = Math.sin(angle);

        drawingCtx.fillStyle = colour;
        drawingCtx.beginPath();
        drawingCtx.moveTo(x, y);
        drawingCtx.lineTo(x - width * cosAngle + height * sineAngle, y - width * sineAngle - height * cosAngle);
        drawingCtx.lineTo(x - width * cosAngle - height * sineAngle, y - width * sineAngle + height * cosAngle);
        drawingCtx.closePath();
        drawingCtx.fill();
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
 * Saves the finite state machine to local storage
 * @param {FiniteStateMachine} fsm 
 */
export function saveFSM(fsm) {
    localStorage["fsm"] = JSON.stringify(fsm);
}

/**
 * Loads Finite State Machine from local storage
 * @returns {FiniteStateMachine}
 */
export function loadFSM() {
    /**@type {string} */
    let fsmJSONText = localStorage["fsm"];
    if (fsmJSONText === undefined) {
        return null;
    };
    let result = JSON.parse(fsmJSONText);
    return result;
}

/**
 * A replace function to remove any null values in JSON text.
 * @param {string} key key of object or index of an array
 * @param {any} value any value
 * @returns Any value that is not not null.
 */
export function ignoreNullOrEmpty(key, value) {
    if (value === null || value === undefined) {
        return undefined
    }
    else if (typeof (value) === "string" && value.trim().length <= 0) {
        return undefined
    }
    else if (Array.isArray(value)) {
        let tempArray = [];
        value.forEach(item => {
            if (item !== null && item !== undefined) {
                tempArray.push(item);
            }
        });
        value = tempArray;
    }

    return value;
}
