import { TextEncoder, TextDecoder } from "util";
import { webcrypto } from "crypto";
import "@testing-library/jest-dom";

global.TextEncoder = TextEncoder;
global.TextDecoder = TextDecoder;

// jsdom has no Web Crypto; serialize-javascript (via react-global-configuration)
// calls crypto.getRandomValues at import time.
if (!globalThis.crypto?.getRandomValues) {
  Object.defineProperty(globalThis, "crypto", {
    value: webcrypto,
    configurable: true,
  });
}
