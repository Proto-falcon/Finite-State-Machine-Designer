const FONTSTYLE = "Times New Roman, serif";
const FONTSIZE = 20;
const CANVASTEXTFONTSTYLE = `${FONTSIZE}px ${FONTSTYLE}`;
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

/** @type {HTMLInputElement} */
let uploadElement = document.getElementById("json-upload");

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
                textY = transition.fromCoord.y - ((FONTSIZE/2) * textLines.length * Math.sin(transition.angle));
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
            caretY = y + (FONTSIZE/2);
        }
        else {
            let initialY = 0;
            if (vertialAlignment == CANVASTEXTVERTICAL.Centre) {
                initialY = y - ((textLines.length - 1) * (FONTSIZE / 2));
            }
            else if (vertialAlignment == CANVASTEXTVERTICAL.Up) {
                initialY = y - ((textLines.length - 1) * FONTSIZE);
            }
            else {
                initialY = y;
            }
            textY = initialY + (FONTSIZE / 2);
            drawingCtx.fillStyle = colour;

            for (var i = 0; i < textLines.length; i++) {
                let textMetric = drawingCtx.measureText(textLines[i]);

                let halfWidth = Math.round(textMetric.width / 2);

                textX = x - halfWidth;
                caretX = x + halfWidth;
                drawingCtx.fillText(textLines[i], textX, textY);

                caretY = textY
                textY += FONTSIZE
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
 * Makes text xml save.
 * @param {string} text 
 * @returns {string} Text that is xml save.
 */
function textToXML(text) {
    text = text.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
    var result = '';
    for (var i = 0; i < text.length; i++) {
        var c = text.charCodeAt(i);
        // Escapes characters such as <, >, &, ", ' and etc.
        if ((c >= 0x20 && c <= 0x7F)
            || c == 0xB2 || c == 0xB3 || c == 0xB9
            || (c >= 0x2074 && c <= 0x2079) || c == 0x2070
            || (c >= 0x2080 && c <= 0x2089)) {
            result += text[i];
        } else {
            result += '&#' + c + ';';
        }
    }
    return result;
}

/**
 * Creates an svg file from Finite State Machine
 * @param {FiniteStateMachine} fsm Finite State machine
 * @param {number} width Width of the svg
 * @param {number} height Height of the svg
 * @param {string} colour Colour to be used in svg file
 * @param {string} backgroundColour Background Colour of svg image
 * @returns {Blob} An svg File
 */
function drawFsmSvg(fsm, width, height, colour, backgroundColour, numPrecision = 2) {
    let svgWidth = width.toFixed(numPrecision);
    let svgHeight = height.toFixed(numPrecision);
    let svgText = `<svg width="${svgWidth}" height="${svgHeight}" xmlns="http://www.w3.org/2000/svg" >`;
    svgText += `<rect width="${svgWidth}" height="${svgHeight}" fill="${backgroundColour}" />`;
    fsm.states.forEach(state => {
        if (state.isDrawable) {
            let stateX = state.coordinate.x.toFixed(numPrecision);
            let stateY = state.coordinate.y.toFixed(numPrecision);
            let stateRadius = state.radius.toFixed(numPrecision);
            svgText += `<circle cx="${stateX}" cy="${stateY}" r="${stateRadius}" `
                + `stroke="${colour}" stroke-width="1" fill="none" />`;
            if (state.isFinalState) {
                svgText += `<circle cx="${stateX}" cy="${stateY}"`
                    + ` r="${(state.radius * FINALSTATECIRCLERATIO).toFixed(numPrecision)}" `
                    + `stroke="${colour}" stroke-width="1" fill="none" />`;
            }
            // loop through text lines to map each to <text/>
            let textLines = state.text.split("\n");
            let textX = "0";
            let initialTextY = state.coordinate.y - ((textLines.length-1) * (FONTSIZE / 2));
            let textY = parseInt((initialTextY + (FONTSIZE / 2)).toFixed(numPrecision));
            textLines.forEach(text => {
                let xmlTxt = textToXML(text);
                textX = (state.coordinate.x - (canvasCtx.measureText(text).width / 2)).toFixed(numPrecision);
                svgText += `<text x="${textX}" y="${textY}" fill="${colour}" font-family="${FONTSTYLE}" font-size="${FONTSIZE}">${xmlTxt}</text>`;
                textY += FONTSIZE;
            });
        }
    });
    //fsm.transitions.forEach(transition => {
    //    svgFile
    //});

    svgText += '</svg>';
    let svgFile = new Blob([svgText], { type: "image/svg+xml" });
    return svgFile;
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
    return JSON.parse(fsmJSONText);
}

/**
 * Downloads a png of the Finite State Machine
 * @param {FiniteStateMachine} fsm
 * @param {string} colour 
 */
export function saveAsPNG(fsm, colour) {
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
 * Downloads an svg file of the finite state machine
 * @param {FiniteStateMachine} fsm Finite state machine
 * @param {string} colour Colour of the finite state machine
 * @param {string} backgroundColour Background colour
 */
export function saveAsSvg(fsm, colour, backgroundColour) {
    let svgBlob = drawFsmSvg(fsm, canvasElement.width, canvasElement.height, colour, backgroundColour);
    downloadFile(svgBlob, "Finite State Machine");
}

/**
 * Downloads a json file of Finite State Machine
 * @param {FiniteStateMachine} fsm
 */
export function saveAsJson(fsm) {
    let fsmJson = new Blob([JSON.stringify(fsm)], {type: "application/json"});
    downloadFile(fsmJson, "Finite State Machine");
}

/**
 * Downloads a blob object with a filename
 * @param {Blob | File} blob
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

export let uploadJson = () => uploadElement.click();

/**
 * Parses json file to object
 * @param {File | Blob} file
 * @returns {Promise<object>}
 */
function parseJson(file) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = (e) => {
            try {
                resolve(JSON.parse(e.target.result));
            } catch (e) { reject(null) }
        }
        reader.onerror = (err) => { reject(err); }
        reader.readAsText(file);
    });
}

/**
 * Loads the json file containing the Finite State Machine in the input[type=file] element.
 * @returns {FiniteStateMachine}
 */
export async function loadJsonUpload() {
    if (uploadElement.files < 1) return;
    let file = uploadElement.files[0];
    let fsm = await parseJson(file);
    return fsm;
}
