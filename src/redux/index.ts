import { configureStore } from "@reduxjs/toolkit";
import createProfleReaducer from "./slices/createProfile"

const store = configureStore({
    reducer:{
        createProfile: createProfleReaducer
    }
})

export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch

export default store

