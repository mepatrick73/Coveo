import { FC } from "react";
import type { Ship as ShipType } from "../../types";
export declare const LifeBar: FC<{
    ship: ShipType;
    currentPercent: number;
    shipSize: number;
}>;
