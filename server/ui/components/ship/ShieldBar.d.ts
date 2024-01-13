import { FC } from "react";
import type { Ship as ShipType } from "../../types";
export declare const ShieldBar: FC<{
    ship: ShipType;
    currentPercent: number;
    shipSize: number;
}>;
