﻿import * as FSMCanvasUtil from "./FSMCanvasUtil.mjs";

/**
 * @typedef {import("./FSMCanvasUtil.mjs").FiniteStateMachine} FiniteStateMachine
 * @typedef {import("./FSMCanvasUtil.mjs").Transition} Transition
 * @typedef {import("./FSMCanvasUtil.mjs").FiniteState} FiniteState
 * @typedef {import("./FSMCanvasUtil.mjs").CanvasCoordinate} CanvasCoordinate
 */

/**
 * Converts FSM in canvas to PNG
 * @param {FiniteStateMachine} fsm Finite State Machine
 * @param {string} colour colour of the FSM to be drawn in
 * @param {number} [scale=1] Scale that the state will be drawn in.
 * @returns {string} Data URL of FSM canvas PNG
 */
export function fsmToPNG(fsm, colour, scale = 1) {
    /** @type {HTMLCanvasElement} */
    let tmpCanvas = document.createElement("canvas");
    tmpCanvas.width = FSMCanvasUtil.canvasElement.width * scale;
    tmpCanvas.height = FSMCanvasUtil.canvasElement.height * scale;
    let tmpCanvasCtx = tmpCanvas.getContext("2d");
    tmpCanvasCtx.font = FSMCanvasUtil.CANVASTEXTFONTSTYLE;

    /** @type {CanvasRenderingContext2D} */
    let pngDrawingCtx = {
        canvas: tmpCanvasCtx.canvas,
        beginPath: () => tmpCanvasCtx.beginPath(),
        closePath: () => tmpCanvasCtx.closePath(),
        get lineWidth() {
            return tmpCanvasCtx.lineWidth;
        },
        set lineWidth(value) {
            tmpCanvasCtx.lineWidth = value;
        },
        get strokeStyle() {
            return tmpCanvasCtx.strokeStyle;
        },
        set strokeStyle(value) {
            tmpCanvasCtx.strokeStyle = value;
        },
        get fillStyle() {
            return tmpCanvasCtx.fillStyle;
        },
        set fillStyle(value) {
            tmpCanvasCtx.fillStyle = value;
        },
        arc: (x, y, radius, startAngle, endAngle, counterClockwise = false) =>
            tmpCanvasCtx.arc(x, y, radius, startAngle, endAngle, counterClockwise),
        moveTo: (x, y) => tmpCanvasCtx.moveTo(x, y),
        lineTo: (x, y) => tmpCanvasCtx.lineTo(x, y),
        stroke: () => tmpCanvasCtx.stroke(),
        measureText: (text) => tmpCanvasCtx.measureText(text),
        fillText: (text, x, y) => {
            tmpCanvasCtx.font = `${FSMCanvasUtil.FONTSIZE * scale}px ${FSMCanvasUtil.FONTSTYLE}`;
            tmpCanvasCtx.fillText(text, x, y);
            tmpCanvasCtx.font = FSMCanvasUtil.CANVASTEXTFONTSTYLE;
        },
        fill: () => tmpCanvasCtx.fill()
    };

    fsm.states.forEach(state =>
        FSMCanvasUtil.drawState(state, colour, false, pngDrawingCtx, scale)
    );
    fsm.transitions.forEach(transition =>
        FSMCanvasUtil.drawTransition(transition, colour, false, pngDrawingCtx, scale)
    );

    return tmpCanvas.toDataURL('image/png');
}

/**
 * Makes text xml save.
 * @param {string} text 
 * @returns {string} Text that is xml save.
 */
export function textToXML(text) {
    text = text.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
    var result = '';
    for (var i = 0; i < text.length; i++) {
        var c = text.charCodeAt(i);
        // Escapes characters such as <, >, &, ", ' and etc.
        if ((c >= 0x20 && c <= 0x7F)
            || c === 0xB2 || c === 0xB3 || c === 0xB9
            || (c >= 0x2074 && c <= 0x2079) || c === 0x2070
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
 * @param {number} [numPrecision=2] The number of decimal places to be used
 * @param {number} [scale=1] Scale the canvas by a factor
 * @param {boolean} [asUrl=false] Flag to export it as blob when false, otherwise data URL;
 * @returns {Blob|string} An SVG file or text
 */
export function fsmToSVG(fsm, width, height, colour, backgroundColour,
    numPrecision = 2, scale = 1, asUrl = false) {
    let svgWidth = (width * scale).toFixed(numPrecision);
    let svgHeight = (height * scale).toFixed(numPrecision);
    let svgText = `<svg width="${svgWidth}" height="${svgHeight}"`
        + ` xmlns="http://www.w3.org/2000/svg" >\n`;
    svgText += `<rect width="${svgWidth}" height="${svgHeight}"`
        + ` fill="${backgroundColour}" />\n`;
    fsm.states.forEach(
        /** @param {FiniteState} state */
        state => {
        if (state.isDrawable) {
            let stateX = (state.coordinate.x * scale).toFixed(numPrecision);
            let stateY = (state.coordinate.y * scale).toFixed(numPrecision);
            let stateRadius = (state.radius * scale).toFixed(numPrecision);
            svgText += `<circle cx="${stateX}" cy="${stateY}" r="${stateRadius}" `
                + `stroke="${colour}" stroke-width="${scale.toFixed(numPrecision)}"`
                + ` fill="none" />\n`;
            if (state.isFinalState) {
                let finalRadius = state.radius * FSMCanvasUtil.FINALSTATECIRCLERATIO * scale;
                svgText += `<circle cx="${stateX}" cy="${stateY}"`
                    + ` r="${finalRadius.toFixed(numPrecision)}" `
                    + `stroke="${colour}" stroke-width="${scale.toFixed(numPrecision)}"`
                    + ` fill="none" />\n`;
            }
            // loop through text lines to map each to <text/>
            let textLines = state.text.split("\n");
            let textX = "0";
            let initialTextY = state.coordinate.y
                - ((textLines.length - 1) * (FSMCanvasUtil.FONTSIZE / 2));
            let numTextY = (initialTextY + (FSMCanvasUtil.FONTSIZE / 2)) * scale;
            let textY = parseInt(numTextY.toFixed(numPrecision));
            textLines.forEach(text => {
                let xmlTxt = textToXML(text);
                let tempX = (state.coordinate.x
                    - (FSMCanvasUtil.canvasCtx.measureText(text).width / 2)) * scale;
                textX = tempX.toFixed(numPrecision);
                svgText += `<text x="${textX}" y="${textY}"`
                    + ` fill="${colour}" font-family="${FSMCanvasUtil.FONTSTYLE}"`
                    + ` font-size="${FSMCanvasUtil.FONTSIZE * scale}">${xmlTxt}</text>\n`;
                textY += FSMCanvasUtil.FONTSIZE * scale;
            });
        }
    });
    fsm.transitions.forEach(
        /** @param {Transition} transition */
        transition => {
            let arrowCoord = new FSMCanvasUtil.CanvasCoordinate();
            let arrowAngle = transition.angle;
            let fromCoord = new FSMCanvasUtil.CanvasCoordinate(
                transition.fromCoord.x * scale, transition.fromCoord.y * scale);
            let toCoord = new FSMCanvasUtil.CanvasCoordinate(
                transition.toCoord.x * scale, transition.toCoord.y * scale);
            let textLines = transition.text.split("\n");
            if (!transition.isCurved) {
                arrowCoord = new FSMCanvasUtil.CanvasCoordinate(fromCoord.x, toCoord.y);
                svgText += `<line x1="${fromCoord.x.toFixed(numPrecision)}"`
                    + ` y1="${fromCoord.y.toFixed(numPrecision)}"`
                    + ` x2="${toCoord.x.toFixed(numPrecision)}"`
                    + ` y2="${toCoord.y.toFixed(numPrecision)}"`
                    + ` stroke="${colour}" stroke-width="${scale.toFixed(numPrecision)}" />\n`;
                arrowCoord = new FSMCanvasUtil.CanvasCoordinate(toCoord.x, toCoord.y);
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
                arrowCoord =  toCoord;
                arrowAngle += (transition.isReversed ? -1 : 1) * (Math.PI / 2);

                /**
                 * Will use path instead as it's actually easier to do
                 * partial circles than with circle element.
                 */
                let scaledTransitionRadius = transition.radius * scale;
                let largeArcFlag = (Math.abs(toAngle - fromAngle) > Math.PI)
                    * !transition.isReversed * 1;
                svgText += `<path fill="none" stroke="${colour}"`
                    + ` stroke-width="${scale.toFixed(numPrecision)}"`
                    + ` d="M ${fromCoord.x.toFixed(numPrecision)}`
                    + ` ${fromCoord.y.toFixed(numPrecision)}`
                    + ` A ${scaledTransitionRadius.toFixed(numPrecision)}`
                    + ` ${scaledTransitionRadius.toFixed(numPrecision)}`
                    + ` ${0} ${largeArcFlag} ${!transition.isReversed * 1}`
                    + ` ${toCoord.x.toFixed(numPrecision)}`
                    + ` ${toCoord.y.toFixed(numPrecision)}" />\n`;
            }

            var cosAngle = Math.cos(arrowAngle);
            var sineAngle = Math.sin(arrowAngle);
            let xa = arrowCoord.x - FSMCanvasUtil.ARROWWIDTH * scale * cosAngle;
            let ya = arrowCoord.y - FSMCanvasUtil.ARROWWIDTH * scale * sineAngle;
            svgText += `<path fill="${colour}" `
                + `d="M ${arrowCoord.x.toFixed(numPrecision)} ${arrowCoord.y.toFixed(numPrecision)} `
                + `L ${(xa + FSMCanvasUtil.ARROWHEIGHT * scale * sineAngle).toFixed(numPrecision)}, `
                + `${(ya - FSMCanvasUtil.ARROWHEIGHT * scale * cosAngle).toFixed(numPrecision)} `
                + `${(xa - FSMCanvasUtil.ARROWHEIGHT * scale * sineAngle).toFixed(numPrecision)}, `
                + `${(ya + FSMCanvasUtil.ARROWHEIGHT * scale * cosAngle).toFixed(numPrecision)} `
                + `Z"/>\n`;

            let textCoordInfo = FSMCanvasUtil
                .SetTransitionTextCoords(transition, FSMCanvasUtil.canvasCtx);

            function ExportAsSvg() {
                this.canvas = FSMCanvasUtil.canvasCtx.canvas;
                this.fillStyle = colour;
                /**
                 * @param {string} text
                 * @returns {TextMetrics}
                 */
                this.measureText = (text) => FSMCanvasUtil.canvasCtx.measureText(text);
                /**
                 * 
                 * @param {string} text
                 * @param {number} x
                 * @param {number} y
                 */
                this.fillText = (text, x, y) => {
                    let xmlText = textToXML(text);
                    svgText += `<text x="${x.toFixed(numPrecision)}" y="${y.toFixed(numPrecision)}" `
                        + `fill="${this.fillStyle}" font-family="${FSMCanvasUtil.FONTSTYLE}" `
                        + `font-size="${FSMCanvasUtil.FONTSIZE * scale}">${xmlText}</text>\n`;
                };
            };

            let drawingCtx = new ExportAsSvg();

            FSMCanvasUtil.drawCanvasText(
                drawingCtx, textCoordInfo.textX, textCoordInfo.textY,
                colour, textLines, false, textCoordInfo.vertialAlignment, scale
            );
        }
    );

    svgText += '</svg>\n';
    let svgFile = new Blob([svgText], { type: "image/svg+xml" });
    if (asUrl) {
        return URL.createObjectURL(svgFile);
    }
    return svgFile;
}
