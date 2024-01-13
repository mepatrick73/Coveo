import { Viewport as PixiViewport } from "pixi-viewport";
import { ReactNode } from "react";
import { Coordinates } from "../../types";
interface TeamPixiViewportProps {
    app: any;
    worldWidth: number;
    worldHeight: number;
    viewportWidth: number;
    viewportHeight: number;
    shipSize: number;
    position: Coordinates;
    offset: Coordinates;
    mode: 2 | 4;
    children: ReactNode;
}
export declare const TeamPixiViewport: import("react").FC<TeamPixiViewportProps & {
    ref?: import("react").Ref<PixiViewport> | undefined;
}>;
export {};
