import { createApp } from "vue";
import App from "./App.vue";
import router from "./router";
import "./style.css";
import { Amplify } from "aws-amplify";
import { VueQueryPlugin } from "@tanstack/vue-query";
import awsconfig from "./aws-exports";
import AmplifyVue from "@aws-amplify/ui-vue";


Amplify.configure(awsconfig);

const app = createApp(App);
app.use(router);
app.use(VueQueryPlugin);
app.use(AmplifyVue);

app.mount("#app");
