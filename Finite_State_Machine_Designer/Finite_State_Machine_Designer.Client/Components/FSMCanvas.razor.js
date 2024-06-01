import * as FSMCanvasUtils from "../JS/FSMCanvasUtil.mjs";

export let fSMCanvasUtils = FSMCanvasUtils;

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
 * Creates an SVG file from Finite State Machine
 * @param {FiniteStateMachine} fsm Finite State machine
 * @param {number} width Width of the SVG
 * @param {number} height Height of the SVG
 * @param {string} colour Colour to be used in SVG file
 * @param {string} backgroundColour Background Colour of SVG image
 * @returns {Blob} An SVG File
 */
function drawFsmSvg(fsm, width, height, colour, backgroundColour, numPrecision = 2) {
    let svgWidth = width.toFixed(numPrecision);
    let svgHeight = height.toFixed(numPrecision);
    let svgText = `<svg width="${svgWidth}" height="${svgHeight}" xmlns="http://www.w3.org/2000/svg" >\n`;
    svgText += `<rect width="${svgWidth}" height="${svgHeight}" fill="${backgroundColour}" />\n`;
    fsm.states.forEach(state => {
        if (state.isDrawable) {
            let stateX = state.coordinate.x.toFixed(numPrecision);
            let stateY = state.coordinate.y.toFixed(numPrecision);
            let stateRadius = state.radius.toFixed(numPrecision);
            svgText += `<circle cx="${stateX}" cy="${stateY}" r="${stateRadius}" `
                + `stroke="${colour}" stroke-width="1" fill="none" />\n`;
            if (state.isFinalState) {
                svgText += `<circle cx="${stateX}" cy="${stateY}"`
                    + ` r="${(state.radius * FSMCanvasUtils.FINALSTATECIRCLERATIO).toFixed(numPrecision)}" `
                    + `stroke="${colour}" stroke-width="1" fill="none" />\n`;
            }
            // loop through text lines to map each to <text/>
            let textLines = state.text.split("\n");
            let textX = "0";
            let initialTextY = state.coordinate.y - ((textLines.length-1) * (FSMCanvasUtils.FONTSIZE / 2));
            let textY = parseInt((initialTextY + (FSMCanvasUtils.FONTSIZE / 2)).toFixed(numPrecision));
            textLines.forEach(text => {
                let xmlTxt = textToXML(text);
                textX = (state.coordinate.x - (FSMCanvasUtils.canvasCtx.measureText(text).width / 2)).toFixed(numPrecision);
                svgText += `<text x="${textX}" y="${textY}" fill="${colour}" font-family="${FSMCanvasUtils.FONTSTYLE}" `
                    + `font-size="${FSMCanvasUtils.FONTSIZE}">${xmlTxt}</text>\n`;
                textY += FSMCanvasUtils.FONTSIZE;
            });
        }
    });
    fsm.transitions.forEach(transition => {
        let arrowCoord = new fSMCanvasUtils.CanvasCoordinate();
        let arrowAngle = transition.angle;
        let fromCoord = transition.fromCoord;
        let toCoord = transition.toCoord;
        let textLines = transition.text.split("\n");
        if (!transition.isCurved) {
            arrowCoord = new fSMCanvasUtils.CanvasCoordinate(fromCoord.x, toCoord.y);
            svgText += `<line x1="${fromCoord.x.toFixed(numPrecision)}" y1="${fromCoord.y.toFixed(numPrecision)}"` +
                ` x2="${toCoord.x.toFixed(numPrecision)}" y2="${toCoord.y.toFixed(numPrecision)}" stroke="${colour}" />\n`;
            arrowCoord = new fSMCanvasUtils.CanvasCoordinate(toCoord.x, toCoord.y);
        }
        else {
            arrowAngle = transition.toAngle;
            arrowCoord = transition.toCoord;
            arrowAngle += (transition.isReversed ? -1 : 1) * (Math.PI / 2);

            let fromAngle = transition.fromAngle;
            let toAngle = transition.toAngle;

            if (toAngle < fromAngle) {
                toAngle += 2 * Math.PI;
            }

            arrowAngle = transition.toAngle;
            arrowCoord = transition.toCoord;
            arrowAngle += (transition.isReversed ? -1 : 1) * (Math.PI / 2);

            // Will use path instead as it's actually easier to do partial circles than with circle element.
            svgText += `<path fill="none" stroke="${colour}"`
                + ` d="M ${fromCoord.x.toFixed(numPrecision)} ${fromCoord.y.toFixed(numPrecision)}`
                + ` A ${transition.radius.toFixed(numPrecision)} ${transition.radius.toFixed(numPrecision)}`
                + ` ${0} ${(Math.abs(toAngle - fromAngle) > Math.PI) * 1} ${1}`
                + ` ${toCoord.x.toFixed(numPrecision)} ${toCoord.y.toFixed(numPrecision)}" />\n`;
        }

        var cosAngle = Math.cos(arrowAngle);
        var sineAngle = Math.sin(arrowAngle);
        let xa = arrowCoord.x - FSMCanvasUtils.ARROWWIDTH * cosAngle;
        let ya = arrowCoord.y - FSMCanvasUtils.ARROWWIDTH * sineAngle;
        svgText += `<path fill="${colour}" `
            + `d="M ${arrowCoord.x.toFixed(numPrecision)} ${arrowCoord.y.toFixed(numPrecision)} `
            + `L ${(xa + FSMCanvasUtils.ARROWHEIGHT * sineAngle).toFixed(numPrecision)}, `
            + `${(ya - FSMCanvasUtils.ARROWHEIGHT * cosAngle).toFixed(numPrecision)} `
            + `${(xa - FSMCanvasUtils.ARROWHEIGHT * sineAngle).toFixed(numPrecision)}, `
            + `${(ya + FSMCanvasUtils.ARROWHEIGHT * cosAngle).toFixed(numPrecision)} `
            + `Z"/>\n`;

        let textCoordInfo = fSMCanvasUtils.SetTransitionTextCoords(transition, FSMCanvasUtils.canvasCtx);

        function ExportAsSvg() {
            this.canvas = FSMCanvasUtils.canvasCtx.canvas;
            this.fillStyle = colour;
            /**
             * @param {string} text
             * @returns {TextMetrics}
             */
            this.measureText = (text) => FSMCanvasUtils.canvasCtx.measureText(text);
            /**
             * 
             * @param {string} text
             * @param {number} x
             * @param {number} y
             */
            this.fillText = (text, x, y) => {
                let xmlText = textToXML(text);
                svgText += `<text x="${x.toFixed(numPrecision)}" y="${y.toFixed(numPrecision)}" `
                    + `fill="${this.fillStyle}" font-family="${FSMCanvasUtils.FONTSTYLE}" `
                    + `font-size="${FSMCanvasUtils.FONTSIZE}">${xmlText}</text>\n`;
            };
        };

        let drawingCtx = new ExportAsSvg();

        fSMCanvasUtils.drawCanvasText(
            drawingCtx, textCoordInfo.textX, textCoordInfo.textY, colour, textLines, false, textCoordInfo.vertialAlignment
        );
    });

    svgText += '</svg>\n';
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
 * Converts FSM in canvas to Data URL
 * @param {FiniteStateMachine} fsm
 * @param {string} colour 
 * @returns {string} Data URL of FSM canvas
 */
function fSMToDataURL(fsm, colour) {
    /** @type {HTMLCanvasElement} */
    let tmpCanvas = document.createElement("canvas");
    tmpCanvas.width = FSMCanvasUtils.canvasElement.width;
    tmpCanvas.height = FSMCanvasUtils.canvasElement.height;
    let tmpCanvasCtx = tmpCanvas.getContext("2d");
    tmpCanvasCtx.font = FSMCanvasUtils.CANVASTEXTFONTSTYLE;

    fsm.states.forEach(state =>
        fSMCanvasUtils.drawState(state, colour, false, tmpCanvasCtx)
    );
    fsm.transitions.forEach(transition =>
        fSMCanvasUtils.drawTransition(transition, colour, false, tmpCanvasCtx)
    );

    return tmpCanvas.toDataURL('image/png');
}

/**
 * Downloads a png of the Finite State Machine
 * @param {FiniteStateMachine} fsm
 * @param {string} colour 
 */
export function saveAsPNG(fsm, colour) {

    let pngData = fSMToDataURL(fsm, colour);
    let anchor = document.createElement('a');
    anchor.href = pngData;
    anchor.download = "Finite State Machine";
    anchor.click();
}

/**
 * Downloads an SVG file of the finite state machine
 * @param {FiniteStateMachine} fsm Finite state machine
 * @param {string} colour Colour of the finite state machine
 * @param {string} backgroundColour Background colour
 */
export function saveAsSvg(fsm, colour, backgroundColour) {
    let svgBlob = drawFsmSvg(fsm, FSMCanvasUtils.canvasElement.width, FSMCanvasUtils.canvasElement.height, colour, backgroundColour);
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

export let uploadJson = () => FSMCanvasUtils.uploadElement.click();

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
    if (FSMCanvasUtils.uploadElement.files < 1) return;
    let file = FSMCanvasUtils.uploadElement.files[0];
    let fsm = await parseJson(file);
    return fsm;
}
