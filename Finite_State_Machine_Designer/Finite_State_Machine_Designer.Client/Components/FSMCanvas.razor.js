const canvasWidthRatio = 0.8;
const canvasHeightRatio = 0.8;

class Coordinate {
    x = -1
    y = -1
}

class State {
    coordinate = new Coordinate();
    radius = 0;
    text = "";
    IsFinal = false;
}


class StateTransition {
    /** @type {?State} */
    fromState = new State();
    toState = new State();
    arcRadius = 0;
    text = "";
}



/**
 * Represents the Canvas element
 */
class Canvas {
    constructor(canvasId) {
        this.canvasExists = false;

        /** @type {HTMLCanvasElement}*/
        this.canvasElement = document.getElementById(canvasId);

        /**@type {CanvasRenderingContext2D} */
        this.canvasCtx;

        if (this.canvasElement.getContext) {
            this.canvasCtx = this.canvasElement.getContext("2d");
            this.canvasExists = true;
        }
    }

    /**
     * Updates the canvas element dimensions
     * @param {number} height
     * @param {number} width
     */
    updateDimensions(height, width) {
        this.canvasElement.width = width;
        this.canvasElement.height = height;

        // TODO: Convert C# objects to js objects to be serialized to string
        // in order to be stored in localstorage

        // Can use IAsyncDisposable on Blazor side to excute code to do above tasks
    }
}

let canvas = new Canvas("FSMCanvas");

/**
 * Clears the whole canvas
 * @returns {boolean} True if cleared the whole canvas successfully,
 * otherwise there wasn't canvas (context) to doesn't exist.
 */
export function clearCanvas() {
    if (canvas.canvasExists) {
        canvas.canvasCtx.clearRect(
            0,
            0,
            canvas.canvasElement.width,
            canvas.canvasElement.height
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
    const height = window.outerHeight * canvasHeightRatio;
    const width = window.outerWidth * canvasWidthRatio;
    canvas.updateDimensions(height, width);
}

addEventListener("resize", updateCanvasDimensions);
//addEventListener("visibilitychange");


/**
 * Creates a state at a position within the canvas with colour.
 * @param {number} x X co-ordinate in canvas space
 * @param {number} y Y co-ordinate in canvas space
 * @param {number} radius Radius of the state
 * @param {string} colour Colour when drawn 
 * @returns {boolean} True when created successfully, otherwise can't create it because canvas (context) doesn't exist.
 */
export function DrawState(x, y, radius, colour) {
    if (canvas.canvasExists) {
        canvas.canvasCtx.beginPath();
        canvas.canvasCtx.strokeStyle = colour;
        canvas.canvasCtx.arc(x, y, radius, 0, 2 * Math.PI);
        canvas.canvasCtx.closePath();
        canvas.canvasCtx.stroke();
        return true;
    }
    return false;
}
