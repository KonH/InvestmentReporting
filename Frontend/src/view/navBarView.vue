<template>
	<nav class="navbar navbar-expand-lg navbar-light bg-light">
		<div class="navbar-nav mr-auto">
			<router-link to="/config" :class="navClass('/config')">Config</router-link>
			<router-link to="/portfolio" :class="navClass('/portfolio')">Portfolio</router-link>
		</div>
	</nav>
</template>
<script lang="ts">
import { Vue } from 'vue-class-component';

export default class App extends Vue {
	navClass(path: string) {
		const isCurrent = path == this.$route.path;
		const inactiveClass = 'nav-item nav-link';
		return isCurrent ? inactiveClass + ' active' : inactiveClass;
	}
}
</script>
