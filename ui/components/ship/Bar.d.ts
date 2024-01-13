import { ColorSource } from "pixi.js";
import { FC } from "react";
import { Coordinates } from "../../types";
interface BarProps {
    currentPercent: number;
    color: ColorSource;
    position: Coordinates;
    length: number;
    thickness: number;
}
export declare const Bar: FC<BarProps>;
export {};
