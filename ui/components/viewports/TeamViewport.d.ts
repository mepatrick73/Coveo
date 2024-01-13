import { FC } from "react";
import { Ship as ShipType } from "../../types";
export declare const TeamViewport: FC<{
    ship: ShipType;
    index: number;
    mode: 2 | 4;
    visible: boolean;
}>;
