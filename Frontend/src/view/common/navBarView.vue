<template>
	<nav class="navbar navbar-expand-lg navbar-light bg-light">
		<div class="navbar-nav mr-auto">
			<router-link to="/config" :class="navClass('/config')">Config</router-link>
			<router-link to="/import" :class="navClass('/import')">Import</router-link>
			<router-link to="/operations" :class="navClass('/operations')">Operations</router-link>
			<router-link to="/portfolio" :class="navClass(['/portfolio', '/'])">Portfolio</router-link>
			<router-link to="/dividends" :class="navClass('/dividends')">Dividends</router-link>
			<router-link to="/coupons" :class="navClass('/coupons')">Coupons</router-link>
			<router-link to="/tags" :class="navClass('/tags')">Tags</router-link>
			<router-link to="/dashboards" :class="navClass('/dashboards')">Dashboards</router-link>
			<router-link to="/custom" :class="navClass('/custom')">Custom</router-link>
		</div>
	</nav>
</template>
<script lang="ts">
import { Vue } from 'vue-class-component';

export default class App extends Vue {
	navClass(path: string | string[]) {
		const isCurrent = this.isCurrent(path);
		const inactiveClass = 'nav-item nav-link';
		return isCurrent ? inactiveClass + ' active' : inactiveClass;
	}

	isCurrent(path: string | string[]) {
		if (typeof path == 'string') {
			return path == this.$route.path;
		}
		for (const p of path) {
			if (this.isCurrent(p)) {
				return true;
			}
		}
		return false;
	}
}
</script>
