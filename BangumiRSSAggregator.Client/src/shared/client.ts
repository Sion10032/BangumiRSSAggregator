import ky, { Options } from "ky";

const option : Options = {
  prefixUrl: "http://localhost:5072/api",
  timeout: 20000,
};

const client = ky.extend(option);

export default client;