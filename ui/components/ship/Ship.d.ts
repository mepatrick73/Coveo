import { FC } from "react";
import { GameConstants, Ship as ShipType } from "../../types";
export declare const Ship: FC<{
    ship: ShipType;
    constants: GameConstants;
    showGrid: boolean;
}>;
