import * as FSMCanvasUtil from "./FSMCanvasUtil.mjs";

/**
 * Converts FSM in canvas to PNG
 * @param {FiniteStateMachine} fsm
 * @param {string} colour 
 * @returns {string} PNG of FSM canvas
 */
export function fsmToPNG(fsm, colour) {
    /** @type {HTMLCanvasElement} */
    let tmpCanvas = document.createElement("canvas");
    tmpCanvas.width = FSMCanvasUtil.canvasElement.width;
    tmpCanvas.height = FSMCanvasUtil.canvasElement.height;
    let tmpCanvasCtx = tmpCanvas.getContext("2d");
    tmpCanvasCtx.font = FSMCanvasUtil.CANVASTEXTFONTSTYLE;

    fsm.states.forEach(state =>
        FSMCanvasUtil.drawState(state, colour, false, tmpCanvasCtx)
    );
    fsm.transitions.forEach(transition =>
        FSMCanvasUtil.drawTransition(transition, colour, false, tmpCanvasCtx)
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
export function fsmToSVG(fsm, width, height, colour, backgroundColour, numPrecision = 2) {
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
                    + ` r="${(state.radius * FSMCanvasUtil.FINALSTATECIRCLERATIO).toFixed(numPrecision)}" `
                    + `stroke="${colour}" stroke-width="1" fill="none" />\n`;
            }
            // loop through text lines to map each to <text/>
            let textLines = state.text.split("\n");
            let textX = "0";
            let initialTextY = state.coordinate.y - ((textLines.length - 1) * (FSMCanvasUtil.FONTSIZE / 2));
            let textY = parseInt((initialTextY + (FSMCanvasUtil.FONTSIZE / 2)).toFixed(numPrecision));
            textLines.forEach(text => {
                let xmlTxt = textToXML(text);
                textX = (state.coordinate.x - (FSMCanvasUtil.canvasCtx.measureText(text).width / 2)).toFixed(numPrecision);
                svgText += `<text x="${textX}" y="${textY}" fill="${colour}" font-family="${FSMCanvasUtil.FONTSTYLE}" `
                    + `font-size="${FSMCanvasUtil.FONTSIZE}">${xmlTxt}</text>\n`;
                textY += FSMCanvasUtil.FONTSIZE;
            });
        }
    });
    fsm.transitions.forEach(transition => {
        let arrowCoord = new FSMCanvasUtil.CanvasCoordinate();
        let arrowAngle = transition.angle;
        let fromCoord = transition.fromCoord;
        let toCoord = transition.toCoord;
        let textLines = transition.text.split("\n");
        if (!transition.isCurved) {
            arrowCoord = new FSMCanvasUtil.CanvasCoordinate(fromCoord.x, toCoord.y);
            svgText += `<line x1="${fromCoord.x.toFixed(numPrecision)}" y1="${fromCoord.y.toFixed(numPrecision)}"` +
                ` x2="${toCoord.x.toFixed(numPrecision)}" y2="${toCoord.y.toFixed(numPrecision)}" stroke="${colour}" />\n`;
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
        let xa = arrowCoord.x - FSMCanvasUtil.ARROWWIDTH * cosAngle;
        let ya = arrowCoord.y - FSMCanvasUtil.ARROWWIDTH * sineAngle;
        svgText += `<path fill="${colour}" `
            + `d="M ${arrowCoord.x.toFixed(numPrecision)} ${arrowCoord.y.toFixed(numPrecision)} `
            + `L ${(xa + FSMCanvasUtil.ARROWHEIGHT * sineAngle).toFixed(numPrecision)}, `
            + `${(ya - FSMCanvasUtil.ARROWHEIGHT * cosAngle).toFixed(numPrecision)} `
            + `${(xa - FSMCanvasUtil.ARROWHEIGHT * sineAngle).toFixed(numPrecision)}, `
            + `${(ya + FSMCanvasUtil.ARROWHEIGHT * cosAngle).toFixed(numPrecision)} `
            + `Z"/>\n`;

        let textCoordInfo = FSMCanvasUtil.SetTransitionTextCoords(transition, FSMCanvasUtil.canvasCtx);

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
                    + `font-size="${FSMCanvasUtil.FONTSIZE}">${xmlText}</text>\n`;
            };
        };

        let drawingCtx = new ExportAsSvg();

        FSMCanvasUtil.drawCanvasText(
            drawingCtx, textCoordInfo.textX, textCoordInfo.textY, colour, textLines, false, textCoordInfo.vertialAlignment
        );
    });

    svgText += '</svg>\n';
    let svgFile = new Blob([svgText], { type: "image/svg+xml" });
    return svgFile;
}