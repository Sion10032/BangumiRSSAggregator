import ky, { Options } from "ky";

const option : Options = {
    prefixUrl: "http://localhost:5072/api"
};

const client = ky.extend(option);

export default client;