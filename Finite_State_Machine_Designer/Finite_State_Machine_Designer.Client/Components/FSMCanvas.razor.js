const canvasWidthRatio = 0.8;
const canvasHeightRatio = 0.8;

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

        // USE getImageData and putImageData to temporarily store canvas when resizing

    }
}

let canvas = new Canvas("FSMCanvas");


/**
 * Updates the canvas dimensions when the window size changes
 * @param {Event} event
 */
function updateCanvasDimensions(event) {
    const height = window.innerHeight * canvasHeightRatio;
    const width = window.innerWidth * canvasWidthRatio;
    canvas.updateDimensions(height, width);
}

addEventListener("resize", updateCanvasDimensions);

/**
 * Creates a state at a position within the canvas with colour.
 * @param {number} x X co-ordinate in canvas space
 * @param {number} y Y co-ordinate in canvas space
 * @param {any} radius Radius of the state
 * @param {any} selectedColour Colour when created 
 * @returns True when created successfully, otherwise can't create it because canvas context is false.
 */
export function CreateState(x, y, radius, selectedColour = "blue") {
    if (canvas.canvasExists) {
        canvas.canvasCtx.beginPath();
        canvas.canvasCtx.strokeStyle = selectedColour;
        canvas.canvasCtx.arc(x, y, radius, 0, 2 * Math.PI);
        canvas.canvasCtx.closePath();
        canvas.canvasCtx.stroke();
        return true
    }
    return false
}

