// BlazorGL Controls - JavaScript Interop
// Handles mouse and touch events for OrbitControls

const controlsState = new Map();

// Mouse button constants
const MOUSE_BUTTON = {
    LEFT: 0,
    MIDDLE: 1,
    RIGHT: 2
};

// Control states
const STATE = {
    NONE: -1,
    ROTATE: 0,
    ZOOM: 1,
    PAN: 2
};

export function initOrbitControls(domElementId, dotNetRef) {
    const element = document.getElementById(domElementId);
    if (!element) {
        console.error(`Element with id '${domElementId}' not found`);
        return;
    }

    const state = {
        dotNetRef: dotNetRef,
        element: element,
        currentState: STATE.NONE,
        rotateStart: { x: 0, y: 0 },
        rotateEnd: { x: 0, y: 0 },
        panStart: { x: 0, y: 0 },
        panEnd: { x: 0, y: 0 },
        pointers: [],
        pointerPositions: {}
    };

    // Event handlers
    const onPointerDown = (event) => handlePointerDown(event, state);
    const onPointerMove = (event) => handlePointerMove(event, state);
    const onPointerUp = (event) => handlePointerUp(event, state);
    const onPointerCancel = (event) => handlePointerUp(event, state);
    const onContextMenu = (event) => event.preventDefault();
    const onWheel = (event) => handleWheel(event, state);

    // Attach event listeners
    element.addEventListener('pointerdown', onPointerDown);
    element.addEventListener('pointermove', onPointerMove);
    element.addEventListener('pointerup', onPointerUp);
    element.addEventListener('pointercancel', onPointerCancel);
    element.addEventListener('contextmenu', onContextMenu);
    element.addEventListener('wheel', onWheel, { passive: false });

    // Store state and handlers for cleanup
    state.handlers = {
        onPointerDown,
        onPointerMove,
        onPointerUp,
        onPointerCancel,
        onContextMenu,
        onWheel
    };

    controlsState.set(domElementId, state);
}

export function disposeOrbitControls(domElementId) {
    const state = controlsState.get(domElementId);
    if (!state) return;

    const element = state.element;
    const handlers = state.handlers;

    // Remove event listeners
    element.removeEventListener('pointerdown', handlers.onPointerDown);
    element.removeEventListener('pointermove', handlers.onPointerMove);
    element.removeEventListener('pointerup', handlers.onPointerUp);
    element.removeEventListener('pointercancel', handlers.onPointerCancel);
    element.removeEventListener('contextmenu', handlers.onContextMenu);
    element.removeEventListener('wheel', handlers.onWheel);

    controlsState.delete(domElementId);
}

function handlePointerDown(event, state) {
    if (state.pointers.length === 0) {
        state.element.setPointerCapture(event.pointerId);
    }

    addPointer(event, state);

    if (event.pointerType === 'touch') {
        handleTouchStart(event, state);
    } else {
        handleMouseDown(event, state);
    }
}

function handlePointerMove(event, state) {
    if (event.pointerType === 'touch') {
        handleTouchMove(event, state);
    } else {
        handleMouseMove(event, state);
    }
}

function handlePointerUp(event, state) {
    removePointer(event, state);

    if (state.pointers.length === 0) {
        state.element.releasePointerCapture(event.pointerId);
        state.currentState = STATE.NONE;
    }
}

function handleMouseDown(event, state) {
    let mouseAction = STATE.NONE;

    switch (event.button) {
        case MOUSE_BUTTON.LEFT:
            mouseAction = STATE.ROTATE;
            break;
        case MOUSE_BUTTON.MIDDLE:
            mouseAction = STATE.ZOOM;
            break;
        case MOUSE_BUTTON.RIGHT:
            mouseAction = STATE.PAN;
            break;
    }

    state.currentState = mouseAction;

    if (mouseAction === STATE.ROTATE) {
        state.rotateStart = { x: event.clientX, y: event.clientY };
        state.rotateEnd = { x: event.clientX, y: event.clientY };
    } else if (mouseAction === STATE.PAN) {
        state.panStart = { x: event.clientX, y: event.clientY };
        state.panEnd = { x: event.clientX, y: event.clientY };
    }
}

function handleMouseMove(event, state) {
    if (state.currentState === STATE.ROTATE) {
        state.rotateEnd = { x: event.clientX, y: event.clientY };

        const deltaX = (state.rotateEnd.x - state.rotateStart.x) * 0.01;
        const deltaY = (state.rotateEnd.y - state.rotateStart.y) * 0.01;

        state.dotNetRef.invokeMethodAsync('OnRotate', deltaX, deltaY);

        state.rotateStart = state.rotateEnd;
    } else if (state.currentState === STATE.PAN) {
        state.panEnd = { x: event.clientX, y: event.clientY };

        const deltaX = state.panEnd.x - state.panStart.x;
        const deltaY = state.panEnd.y - state.panStart.y;

        state.dotNetRef.invokeMethodAsync('OnPan', deltaX, -deltaY);

        state.panStart = state.panEnd;
    }
}

function handleTouchStart(event, state) {
    trackPointer(event, state);

    switch (state.pointers.length) {
        case 1:
            // Single touch - rotate
            state.currentState = STATE.ROTATE;
            state.rotateStart = getSecondPointerPosition(event, state);
            state.rotateEnd = getSecondPointerPosition(event, state);
            break;

        case 2:
            // Two touches - zoom and pan
            state.currentState = STATE.ZOOM;
            const dx = state.pointers[0].pageX - state.pointers[1].pageX;
            const dy = state.pointers[0].pageY - state.pointers[1].pageY;
            state.zoomStart = Math.sqrt(dx * dx + dy * dy);

            const x = (state.pointers[0].pageX + state.pointers[1].pageX) * 0.5;
            const y = (state.pointers[0].pageY + state.pointers[1].pageY) * 0.5;
            state.panStart = { x, y };
            state.panEnd = { x, y };
            break;
    }
}

function handleTouchMove(event, state) {
    trackPointer(event, state);

    switch (state.pointers.length) {
        case 1:
            // Single touch - rotate
            if (state.currentState === STATE.ROTATE) {
                state.rotateEnd = getSecondPointerPosition(event, state);

                const deltaX = (state.rotateEnd.x - state.rotateStart.x) * 0.01;
                const deltaY = (state.rotateEnd.y - state.rotateStart.y) * 0.01;

                state.dotNetRef.invokeMethodAsync('OnRotate', deltaX, deltaY);

                state.rotateStart = state.rotateEnd;
            }
            break;

        case 2:
            // Two touches - zoom and pan
            if (state.currentState === STATE.ZOOM) {
                const dx = state.pointers[0].pageX - state.pointers[1].pageX;
                const dy = state.pointers[0].pageY - state.pointers[1].pageY;
                const zoomEnd = Math.sqrt(dx * dx + dy * dy);

                const zoomDelta = (state.zoomStart - zoomEnd) * 0.01;
                state.dotNetRef.invokeMethodAsync('OnZoom', zoomDelta);

                state.zoomStart = zoomEnd;

                // Pan
                const x = (state.pointers[0].pageX + state.pointers[1].pageX) * 0.5;
                const y = (state.pointers[0].pageY + state.pointers[1].pageY) * 0.5;
                state.panEnd = { x, y };

                const panDeltaX = state.panEnd.x - state.panStart.x;
                const panDeltaY = state.panEnd.y - state.panStart.y;

                state.dotNetRef.invokeMethodAsync('OnPan', panDeltaX, -panDeltaY);

                state.panStart = state.panEnd;
            }
            break;
    }
}

function handleWheel(event, state) {
    event.preventDefault();

    let delta = 0;

    if (event.deltaY !== 0) {
        // Mouse wheel
        delta = event.deltaY * 0.01;
    } else if (event.deltaX !== 0) {
        // Horizontal scroll
        delta = event.deltaX * 0.01;
    }

    state.dotNetRef.invokeMethodAsync('OnZoom', delta);
}

// Pointer tracking helpers
function addPointer(event, state) {
    state.pointers.push(event);
}

function removePointer(event, state) {
    delete state.pointerPositions[event.pointerId];

    for (let i = 0; i < state.pointers.length; i++) {
        if (state.pointers[i].pointerId === event.pointerId) {
            state.pointers.splice(i, 1);
            return;
        }
    }
}

function trackPointer(event, state) {
    let position = state.pointerPositions[event.pointerId];

    if (position === undefined) {
        position = { x: 0, y: 0 };
        state.pointerPositions[event.pointerId] = position;
    }

    position.x = event.pageX;
    position.y = event.pageY;
}

function getSecondPointerPosition(event, state) {
    const pointer = event.pointerId === state.pointers[0].pointerId ?
        state.pointers[1] : state.pointers[0];

    return state.pointerPositions[pointer.pointerId];
}
