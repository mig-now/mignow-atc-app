import { setupServer } from "msw/node";
import { baselineHandlers } from "./handlers";

export const server = setupServer(...baselineHandlers);
