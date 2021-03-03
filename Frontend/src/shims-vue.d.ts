declare module '*.vue' {
  import { DefineComponent } from 'vue';
  const component: DefineComponent<string, never, unknown>;
  export default component;
}
