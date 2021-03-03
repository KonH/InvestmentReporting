<template>
  <div>...</div>
</template>
<script lang="ts">
import { Vue } from 'vue-class-component';

export default class App extends Vue {
  async mounted() {
    const result = await fetch('api/test/v1/hello');
    const status = result.status;
    const statusText = result.statusText;
    const text = result.ok ? await result.text() : '';
    alert(`${status} ${statusText} ${text}`);
  }
}
</script>
