<template>
	<div class="container">
		<router-view />
	</div>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { Action } from 'vuex-class';

@Options({
	name: 'App',
})
export default class App extends Vue {
	@Action('fetchState')
	fetchState!: () => void;

	created() {
		this.fetchState();
	}
}
</script>
