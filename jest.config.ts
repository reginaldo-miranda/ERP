//import { Transform } from "stream";

export default {
    testEnvironment: 'jsdom',
    transform:{
        '^.+\\.jsx?$': 'ts-jest',
    },
    moduleNameMapper:{
        '\\.(css|less | sass | scss)$': 'identity-obj-proxy',
        '^.+\\.svg$': ' jest-transformer-svg',
        '^@/(.*)$': '<rootDir>/src/$1'
     },
     setupFilesAfterEnv: ['<rootDir>/jest.setup.ts'],

}